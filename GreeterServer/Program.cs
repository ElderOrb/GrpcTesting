// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Test.Configuration;

namespace GreeterServer
{
    class GrpcLogRedirector : Grpc.Core.Logging.ILogger
    {
        static StreamWriter sw = File.CreateText("server.grpc.log");

        public GrpcLogRedirector()
        {
        }

        public GrpcLogRedirector(string name)
        {
        }

        public void Debug(string message)
        {
            lock(this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Debug: " + message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Debug: " + String.Format(format, formatArgs));
        }

        public void Error(string message)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Error: " + message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Error: " + String.Format(format, formatArgs));
        }

        public void Error(Exception exception, string message)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Error: " + message, exception);
        }

        public Grpc.Core.Logging.ILogger ForType<T>()
        {
            return new GrpcLogRedirector(typeof(T).Name);
        }

        public void Info(string message)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Info: " + message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Info: " + String.Format(format, formatArgs));
        }

        public void Warning(string message)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Warning: " + message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Warning: " + String.Format(format, formatArgs));
        }

        public void Warning(Exception exception, string message)
        {
            lock (this)
                sw.WriteLine(DateTime.Now.ToString() + "|" + "Warning: " + message, exception);
        }
    }

    class GreeterImpl : Greeter.GreeterBase
    {
        private CancellationToken _cancellationToken;

        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Thread.Sleep(1000);
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }

        public override async Task Communicate2(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Reply> responseStream, ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext(_cancellationToken))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (requestStream.Current.TypeCase == Request.TypeOneofCase.Register)
                    {
                        var register = requestStream.Current.Register;

                        var registerReply = new Reply
                        {
                            Register = new Reply.Types.RegisterAck
                            {
                                Endpoint = register.CliendId,
                                Message = "Register done"
                            }
                        };

                        // register & report back fist
                        var registerReplyTask = responseStream.WriteAsync(registerReply).ContinueWith(t =>
                        {
                        });

                        // ... then wait till reply completion
                        await registerReplyTask;
                    }
                }
            }

            catch (Exception ex)
            {
                throw;
            }

            finally
            {
            }
        }

        public override async Task Communicate3(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Reply> responseStream, ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext(_cancellationToken))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (requestStream.Current.TypeCase == Request.TypeOneofCase.Register)
                    {
                        var register = requestStream.Current.Register;

                        var registerReply = new Reply
                        {
                            Register = new Reply.Types.RegisterAck
                            {
                                Endpoint = register.CliendId,
                                Message = "Register done"
                            }
                        };

                        // register & report back fist
                        var registerReplyTask = responseStream.WriteAsync(registerReply).ContinueWith(t =>
                        {
                        });

                        // ... then wait till reply completion
                        await registerReplyTask;
                    }
                }
            }

            catch (Exception ex)
            {
                throw;
            }

            finally
            {
            }
        }

        public override async Task Communicate4(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Reply> responseStream, ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext(_cancellationToken))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (requestStream.Current.TypeCase == Request.TypeOneofCase.Register)
                    {
                        var register = requestStream.Current.Register;

                        var registerReply = new Reply
                        {
                            Register = new Reply.Types.RegisterAck
                            {
                                Endpoint = register.CliendId,
                                Message = "Register done"
                            }
                        };

                        // register & report back fist
                        var registerReplyTask = responseStream.WriteAsync(registerReply).ContinueWith(t =>
                        {
                        });

                        // ... then wait till reply completion
                        await registerReplyTask;
                    }
                }
            }

            catch (Exception ex)
            {
                throw;
            }

            finally
            {
            }
        }

        public override async Task Communicate(IAsyncStreamReader<Request> requestStream,IServerStreamWriter<Reply> responseStream, ServerCallContext context)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | " + "Entering AgentService.Communicate...");
            var peer = context.Peer;
            var peerId = context.Peer.Split(':')[1];
            Program.ActiveHosts.AddOrUpdate(peerId, 1, (key, v) =>
            {
                return v + 1;
            });
            Program.ActivePeers.AddOrUpdate(peer, 1, (key, v) =>
            {
                return v + 1;
            });

            try
            {
                Console.WriteLine(DateTime.Now.ToString() + " | " + "Start listening AgentService.Communicate requests for: {0}", context.Peer);

                while (await requestStream.MoveNext(_cancellationToken))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " | " + "Cancelling AgentServices.Communicate...");
                        break;
                    }

                    Console.WriteLine(DateTime.Now.ToString() + " | " + "Got from Client: {0}", requestStream.Current.TypeCase);

                    if (requestStream.Current.TypeCase == Request.TypeOneofCase.Register)
                    {
                        var register = requestStream.Current.Register;

                        Console.WriteLine(DateTime.Now.ToString() + " | " + "Handling register request from: clentId = {0}, peerId = {1}",
                            register.CliendId, context.Peer);

                        var registerReply = new Reply
                        {
                            Register = new Reply.Types.RegisterAck
                            {
                                Endpoint = register.CliendId, 
                                Message = "Register done"
                            }
                        };

                        Console.WriteLine(DateTime.Now.ToString() + " | " + "Replying back to client");
                        // register & report back fist
                        var registerReplyTask = responseStream.WriteAsync(registerReply).ContinueWith(t =>
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " | " + "Replying back to client.... done");
                        });

                        // ... then wait till reply completion
                        await registerReplyTask;
                    }
                    else if (requestStream.Current.TypeCase == Request.TypeOneofCase.Unregister)
                    {
                        var unregister = requestStream.Current.Unregister;

                        Console.WriteLine(DateTime.Now.ToString() + " | " + "Handling unregister request from: clentId = {0}, peerId = {1}",
                            unregister.ClientId, context.Peer);

                        var unregisterReplyTask = responseStream.WriteAsync(new Reply
                        {
                            Unregister = new Reply.Types.UnRegisterAck
                            {
                                Message = string.Format("Good bye from EM: {0}", context.Host)
                            }
                        });

                        // and wait for reply finishes
                        await unregisterReplyTask;
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " | " + "Exception in AgentService.Communicate for {0}: {1}", context.Peer, ex);
                throw;
            }

            finally
            {
                Program.ActiveHosts.AddOrUpdate(peerId, -1, (key, v) =>
                {
                    return v - 1;
                });
                Program.ActivePeers.AddOrUpdate(peer, -1, (key, v) =>
                {
                    return v - 1;
                });

                Console.WriteLine(DateTime.Now.ToString() + " | " + "Leaving AgentService.Communicate for {0}", context.Peer);
            }
        }

    }

    class Program
    {
        const int Port = 30051;

        public static ConcurrentDictionary<String, int> ActiveHosts = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<String, int> ActivePeers = new ConcurrentDictionary<string, int>();

        public static void Main(string[] args)
        {
            var threadPoolSizeString = ConfigurationManager.AppSettings["grpcThreadPoolSize"];
            if (!String.IsNullOrEmpty(threadPoolSizeString) && int.TryParse(threadPoolSizeString, out var threadPoolSize))
            {
                Console.WriteLine("Overriding grpc server threads count: {0}", threadPoolSize);
                GrpcEnvironment.SetThreadPoolSize(threadPoolSize);
            }

            var completionQueueCountString = ConfigurationManager.AppSettings["grpcCompletionQueueCount"];
            if (!String.IsNullOrEmpty(completionQueueCountString) && int.TryParse(completionQueueCountString, out var completionQueueCount))
            {
                Console.WriteLine("Overriding grpc server completions queues count: {0}", completionQueueCount);
                GrpcEnvironment.SetCompletionQueueCount(completionQueueCount);
            }

            var grpcInlineHandlersString = ConfigurationManager.AppSettings["grpcInlineHandlers"];
            if (!String.IsNullOrEmpty(grpcInlineHandlersString) && bool.TryParse(grpcInlineHandlersString, out var grpcInlineHandlers))
            {
                Console.WriteLine("Overriding grpc server inline handlers: {0}", grpcInlineHandlers);
                GrpcEnvironment.SetHandlerInlining(grpcInlineHandlers);
            }

            var grpcTraceString = ConfigurationManager.AppSettings["grpcTrace"];
            if (!String.IsNullOrEmpty(grpcTraceString))
            {
                Console.WriteLine("Specifying GRPC_TRACE: {0}", grpcTraceString);
                Environment.SetEnvironmentVariable("GRPC_TRACE", grpcTraceString);
            }

            var grpcVerbosityString = ConfigurationManager.AppSettings["grpcVerbosity"];
            if (!String.IsNullOrEmpty(grpcVerbosityString))
            {
                Console.WriteLine("Specifying GRPC_VERBOSITY: {0}", grpcVerbosityString);
                Environment.SetEnvironmentVariable("GRPC_VERBOSITY", grpcVerbosityString);
            }

            var grpcRedirectLogString = ConfigurationManager.AppSettings["grpcRedirectLog"];
            if (!String.IsNullOrEmpty(grpcRedirectLogString) && bool.TryParse(grpcRedirectLogString, out var grpcRedirectLog))
            {
                Console.WriteLine("Redirecting grpc server logging to EM: {0}", grpcRedirectLog);
                if (grpcRedirectLog)
                {
                    GrpcEnvironment.SetLogger(new GrpcLogRedirector());
                }
            }

            List<ChannelOption> defaultChannelOptions = new List<ChannelOption>
            {
                // new ChannelOption(ChannelOptions.SslTargetNameOverride, "WS1201-001"), 
                // new ChannelOption("grpc.disable_client_authority_filter", 1)

                new ChannelOption("grpc.keepalive_time_ms", 2000),
                new ChannelOption("grpc.keepalive_timeout_ms", 10000),
                new ChannelOption("grpc.keepalive_permit_without_calls", 1),
                /*
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 3000), // Minimum time between sending successive ping frames without receiving any data frame, Int valued, milliseconds.
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 3000), // Minimum allowed time between a server receiving successive ping frames without sending any data frame. 
                */
                new ChannelOption("grpc.http2.max_pings_without_data", 0), // How many misbehaving pings the server can bear before 
                                                                            // sending goaway and closing the transport? (0 indicates that the server can bear an infinite number of misbehaving pings)
                new ChannelOption("grpc.enable_deadline_checking", 1),
                new ChannelOption("grpc.max_receive_message_length", -1),
                new ChannelOption("grpc.max_send_message_length", -1),
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 2000),
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms",  10000),
            };

            var cu = new ConfigUtil();
            cu.Initialize();
            var typedGrpcSettings = cu.GetSettingsByPrefix("grpc");

            var channelOptions = new List<ChannelOption>();

            {
                if (typedGrpcSettings != null && typedGrpcSettings.Count != 0)
                {
                    channelOptions = new List<ChannelOption>();
                    typedGrpcSettings.ForEach(o =>
                    {
                        var value = o.Value;
                        var valueType = value.GetType();

                        if (valueType == typeof(int))
                            channelOptions.Add(new ChannelOption(o.Key, (int)value));
                        else
                            channelOptions.Add(new ChannelOption(o.Key, value.ToString()));
                    });
                }
            }

            var _options = channelOptions ?? defaultChannelOptions;

            Server server = new Server(_options)
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("0.0.0.0", args.Length == 0 ? Port : Int32.Parse(args[0]), ServerCredentials.Insecure) }
            };

            server.Start();


            var oldActiveHostsCount = -1;
            var oldActivePeersCount = -1;

            Task.Run(() =>
            {
                while (true)
                {
                    var hosts = ActiveHosts.ToArray();
                    var newActiveHostsCount = 0;
                    foreach(var host in hosts)
                    {
                        newActiveHostsCount += host.Value != 0 ? 1 : 0;
                    }
                    if(newActiveHostsCount != oldActiveHostsCount)
                    {
                        Console.WriteLine("Active hosts: {0} {1}", newActiveHostsCount, string.Join(",", hosts.Where(p => p.Value != 0).Select(h => h.Key)));
                        oldActiveHostsCount = newActiveHostsCount;
                    }

                    var peers = ActivePeers.ToArray();
                    var newActivePeersCount = 0;
                    foreach(var peer in peers)
                    {
                        newActivePeersCount += peer.Value;
                    }
                    if (newActivePeersCount != oldActivePeersCount)
                    {
                        Console.WriteLine("Active peers: {0} {1}", newActivePeersCount, string.Join(", ", peers.Where(p => p.Value != 0).Select(p => p.Key)));
                        oldActivePeersCount = newActivePeersCount;
                    }
                    Thread.Sleep(1000);
                }
            });

            Console.WriteLine("Greeter server listening on port " + (args.Length == 0 ? Port : Int32.Parse(args[0])));
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
