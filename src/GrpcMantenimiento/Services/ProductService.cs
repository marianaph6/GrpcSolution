using Grpc.Core;
using GrpcMantenimiento.Data;
using GrpcMantenimiento.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcMantenimiento.Services
{
    public class ProductService   : ProducIT.ProducITBase                    
    {
        private readonly GrpcDbContext _dbContext;
        var SECRET_KEY = "admin123";

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
    
        public override async Task<ReadProductResponse> ReadProduct (ReadProductRequest readProductRequest, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(readProductRequest.Id.ToString()))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,"Ingrese un Id de valido para un producto"));
            }

            if(readProductRequest.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El indice del Id debe der mayor a 0"));
            }

            //Product product = await _dbContext.Products.FindAsync(readProductRequest.Id);

            Product product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == readProductRequest.Id) ?? throw new RpcException(new Status(StatusCode.NotFound,$"El producto con el Id {readProductRequest.Id} no existe"));
            return await Task.FromResult(
                new ReadProductResponse
                {
                    Id= product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Status = product.Status,
                   
                }
                );

        }


        public override async Task<GetAllResponse> ListProduct(GetAllRequest getAllRequest, ServerCallContext context)
        {
            var response = new GetAllResponse();
            List<Product> products = await _dbContext.Products.ToListAsync();

            foreach(var product in products)
            {
                response.Product.Add(new ReadProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Status = product.Status,
                });
            }

            return await Task.FromResult( response );
        }



        public override async Task<UpdateProductResponse> UpdateProduct(UpdateProductRequest updateProductRequest, ServerCallContext context)
        {
            if(
                updateProductRequest.Id <= 0 
                ||string.IsNullOrEmpty( updateProductRequest.Name ) 
                || string.IsNullOrEmpty(updateProductRequest.Description)
              )
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Debe ingresar los datos correctos"));
            }

            Product product = await _dbContext.Products.FirstOrDefaultAsync(
                x => x.Id == updateProductRequest.Id
                ) ?? throw new RpcException(new Status(StatusCode.NotFound, $"El id {updateProductRequest.Id} del producto ingresado no fue encontrado"));

            product.Name = updateProductRequest.Name;
            product.Description = updateProductRequest.Description;
            product.Status = updateProductRequest.Status;

            await _dbContext.SaveChangesAsync();

            return await Task.FromResult( new UpdateProductResponse
            {
                Id= product.Id,
            } );

        }

        public override async Task<DeleteProductResponse> DeleteProduct (DeleteProductRequest deleteProductRequest, ServerCallContext context)
        {
            if(deleteProductRequest.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El Id del producto ingresado no es valido"));
            }

            Product product = await _dbContext.Products.FirstOrDefaultAsync(
                x => x.Id == deleteProductRequest.Id
                ) ?? throw new RpcException(new Status(StatusCode.NotFound, $"No se encontró ningun producto que corresponda al Id {deleteProductRequest.Id}"));

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new DeleteProductResponse { 
                Id= product.Id 
            } );
        }


    }

    
}
