using Grpc.Core;
using GrpcMantenimiento.Data;
using GrpcMantenimiento.Models;

namespace GrpcMantenimiento.Services
{
    public class ProductService   : ProducIT.ProducITBase                    
    {
        private readonly GrpcDbContext _dbContext;

        public ProductService(GrpcDbContext dbContext) => _dbContext = dbContext;

        public override async Task<CreateProductResponse> CreateProduct( CreateProductRequest createProductRequest, ServerCallContext context)
        {
            // Validar campos
            if(string.IsNullOrEmpty(createProductRequest.Name) || string.IsNullOrEmpty(createProductRequest.Description))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Envie los datos correctamente"));
            }

            //Crear objeto para el db context
            Product product = new()
            {
                Name = createProductRequest.Name,
                Description = createProductRequest.Description
            };

            // Crear Producto
            await _dbContext.AddAsync(product); 
            
            // Guardar y confirmar cambios en el dbContext
            await _dbContext.SaveChangesAsync(); 

            // Retornar la data segun el la estructura del proto
            return await Task.FromResult(

                new CreateProductResponse
                {
                    Id = product.Id
                }
            );
        }
    }

    
}