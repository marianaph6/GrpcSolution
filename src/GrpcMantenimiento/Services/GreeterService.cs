using Grpc.Core;
using GrpcMantenimiento;

namespace GrpcMantenimiento.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var Password = "admin123";
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
