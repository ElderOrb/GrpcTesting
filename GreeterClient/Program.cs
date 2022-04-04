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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Test.Configuration;

namespace GreeterClient
{
    class GrpcLogRedirector : Grpc.Core.Logging.ILogger
    {
        static StreamWriter sw = File.CreateText("client.grpc.log");

        public GrpcLogRedirector()
        {
        }

        public GrpcLogRedirector(string name)
        {
        }

        public void Debug(string message)
        {
            lock (this)
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

    class Program
    {
        public static void Main(string[] args)
        {
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

            List<ChannelOption> _defaultOptions = new List<ChannelOption>
            {
                // new ChannelOption(ChannelOptions.SslTargetNameOverride, "WS1201-001"), 
                // new ChannelOption("grpc.disable_client_authority_filter", 1)

                new ChannelOption("grpc.keepalive_time_ms", 2000), // After a duration of this time the client/server pings its peer to see if the
                                                                    // transport is still alive. Int valued, milliseconds.
                new ChannelOption("grpc.keepalive_timeout_ms", 10000), // After waiting for a duration of this time, if the keepalive ping sender does 
                                                                        // not receive the ping ack, it will close the transport. Int valued, milliseconds.
                new ChannelOption("grpc.keepalive_permit_without_calls", 1), // allow keepalive pings when there's no gRPC calls

                /*
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 3000), // Minimum time between sending successive ping frames without receiving any data frame, Int valued, milliseconds.
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 3000), // Minimum allowed time between a server receiving successive ping frames without sending any data frame. 
                */

                new ChannelOption("grpc.http2.max_pings_without_data", 0), // allow unlimited amount of keepalive pings without data
                new ChannelOption("grpc.max_receive_message_length", -1),
                new ChannelOption("grpc.max_send_message_length", -1)
            };

            var _options = new List<ChannelOption>();

            {
                var cu = new ConfigUtil();
                cu.Initialize();
                var typedGrpcSettings = cu.GetSettingsByPrefix("grpc");

                if (typedGrpcSettings != null && typedGrpcSettings.Count != 0)
                {
                    _options = new List<ChannelOption>();
                    typedGrpcSettings.ForEach(o =>
                    {
                        var value = o.Value;
                        var valueType = value.GetType();

                        if (valueType == typeof(int))
                            _options.Add(new ChannelOption(o.Key, (int)value));
                        else
                            _options.Add(new ChannelOption(o.Key, value.ToString()));
                    });
                }
            }

            Channel channel;
            {
                var options = new List<ChannelOption>(_options ?? _defaultOptions);
                channel = new Channel(args.Length == 0 ? "127.0.0.1:30051" : args[0], ChannelCredentials.Insecure, options);
            }

            Console.WriteLine(DateTime.Now.ToString() + "|" + "channel: {0}", channel.Target);

            var client = new Greeter.GreeterClient(channel);

            while (true)
            {
                String user = "you";

                var cancellationTokenSource = new CancellationTokenSource();

                var register2Task = Task.Run(async () =>
                {
                    try
                    {
                        using (var call = client.Communicate2(null, null, cancellationTokenSource.Token))
                        {
                            var message = new Request
                            {
                                Register = new Request.Types.Register
                                {
                                    CliendId = Environment.MachineName
                                }
                            };

                            await await Task.WhenAny(call.RequestStream.WriteAsync(message), cancellationTokenSource.Token.WhenCanceled());

                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                    }
                });

                var register3Task = Task.Run(async () =>
                {
                    try
                    {
                        using (var call = client.Communicate3(null, null, cancellationTokenSource.Token))
                        {
                            var message = new Request
                            {
                                Register = new Request.Types.Register
                                {
                                    CliendId = Environment.MachineName
                                }
                            };

                            await await Task.WhenAny(call.RequestStream.WriteAsync(message), cancellationTokenSource.Token.WhenCanceled());

                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                    }
                });

                var register4Task = Task.Run(async () =>
                {
                    try
                    {
                        using (var call = client.Communicate4(null, null, cancellationTokenSource.Token))
                        {
                            var message = new Request
                            {
                                Register = new Request.Types.Register
                                {
                                    CliendId = Environment.MachineName
                                }
                            };

                            await await Task.WhenAny(call.RequestStream.WriteAsync(message), cancellationTokenSource.Token.WhenCanceled());

                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                    }
                });

                var registerTask = Task.Run(async () =>
                {
                    try
                    {
                        using (var call = client.Communicate(null, null, cancellationTokenSource.Token))
                        {
                            Console.WriteLine(DateTime.Now.ToString() + "|" + "starting Communicate for {0}", Environment.MachineName);

                            var message = new Request
                            {
                                Register = new Request.Types.Register
                                {
                                    CliendId = Environment.MachineName
                                }
                            };

                            /*
                            for (var j = 0; j < 10; ++j)
                            {
                                Task.Run(() =>
                                {
                                    while (true)
                                    {
                                        var myreply = client.SayHello(new HelloRequest { Name = user });
                                        Console.WriteLine("Greeting: " + myreply.Message + " {0}", DateTime.Now);
                                    }
                                });
                            }
                            */

                            Console.WriteLine(DateTime.Now.ToString() + "|" + "Sending Register request to Server... ");
                            await await Task.WhenAny(call.RequestStream.WriteAsync(message), cancellationTokenSource.Token.WhenCanceled());

                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                                var reply = call.ResponseStream.Current;

                                if (reply.TypeCase == Reply.TypeOneofCase.Register)
                                {
                                    Console.WriteLine(DateTime.Now.ToString() + "|" + "Got Register reply from Server: {0} / {1}", reply.Register.Endpoint, reply.Register.Message);
                                }
                                else if (reply.TypeCase == Reply.TypeOneofCase.Unregister)
                                {
                                    Console.WriteLine(DateTime.Now.ToString() + "|" + "Got UnRegister reply from Server: {0}", reply.Register.Endpoint);
                                }
                            }

                            Console.WriteLine(DateTime.Now.ToString() + "|" + "leaving Communicate for {0}", Environment.MachineName);
                        }
                    }

                    catch(Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + "|" + "{0}", ex);
                    }

                });

                var task = Task.Run(async () =>
                {
                    try
                    {
                        while(true)
                        {
                            client.SayHello(new HelloRequest
                            {

                            });

                            await Task.Delay(30 * 1000);
                        }
                    }

                    catch(Exception ex)
                    {

                    }
                });

                Task.WaitAll(new[] { registerTask, register2Task, register3Task, register4Task });
            }

            Console.WriteLine(DateTime.Now.ToString() + "|" + "Press any key to exit...");
            Console.ReadKey();
        }
    }
}
