using Project_X_Data.Data;
using Project_X_Data.Models.Api;
using Project_X_Data.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Project_X_Data.Models.Api
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(
            IStorageService storageService,
            DataContext dataContext) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;

        //[HttpPost]
        //public object AddProduct(ApiProductFormModel model)
        //{
        //    // Валідація моделі
        //    if (!ModelState.IsValid)
        //    {
        //        return new
        //        {
        //            status = "Validation failed",
        //            errors = ModelState
        //                .Where(e => e.Value?.Errors.Count > 0)
        //                .ToDictionary(
        //                    e => e.Key,
        //                    e => e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
        //                ),
        //            code = 400
        //        };
        //    }
        //    Guid groupGuid;
        //    try { groupGuid = Guid.Parse(model.GroupId); }
        //    catch { return new { status = "Invalid GroupId", code = 400 }; }
        //    _dataContext.Products
        //        .Add(new()
        //        {
        //            Id = Guid.NewGuid(),
        //            GroupId = groupGuid,
        //            Name = model.Name,
        //            Description = model.Description,
        //            Slug = model.Slug,
        //            Price = model.Price,
        //            Stock = model.Stock,
        //            ImageUrl = model.Image == null ? null : _storageService.Save(model.Image)
        //        });

        //    try
        //    {
        //        _dataContext.SaveChanges();
        //        return new { status = "OK", code = 200 };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { status = ex.Message, code = 500 };
        //    }
        //}
    }
}
