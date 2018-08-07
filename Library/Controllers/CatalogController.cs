using Microsoft.AspNetCore.Mvc;
using LibraryData;
using System.Linq;
using Library.Models.Catalog;
using Library.Models.CheckoutModels;

namespace Library.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _assetsService;
        private ICheckout _checkoutsService;

        public CatalogController(ILibraryAsset assets, ICheckout checkouts) {
            _assetsService = assets;
            _checkoutsService = checkouts;
        }

        public IActionResult Index() {
            var assetModels = _assetsService.GetAll();

            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _assetsService.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _assetsService.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _assetsService.GetType(result.Id)
                });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }

        public IActionResult Detail(int id) {

            var asset = _assetsService.GetById(id);

            var currentHolds = _checkoutsService.GetCurrentHolds(id)
                .Select(a=> new AssetHoldModel {
                    HoldPlaced = _checkoutsService.GetCurrentHoldPlaced(a.Id),
                    PatronName = _checkoutsService.GetCurrentHoldPatronName(a.Id)
                });

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Type = _assetsService.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assetsService.GetAuthorOrDirector(id),
                CurrentLocation = _assetsService.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assetsService.GetDeweyIndex(id),
                CheckoutHistory = _checkoutsService.GetCheckoutHistories(id),
                ISBN = _assetsService.GetIsbn(id),
                LastestCheckout = _checkoutsService.GetLatestCheckout(id),
                PatronName = _checkoutsService.GetCurrentCheckoutPatron(id),
                CurrentHolds = currentHolds
            };

            return View(model);
        }

        public IActionResult Checkout(int id)
        {
            var asset = _assetsService.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkoutsService.IsCheckedOut(id)
            };

            return View(model);
        }

        public IActionResult Hold(int id)
        {
            var asset = _assetsService.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.ImageUrl,
                LibraryCardId = "",
                IsCheckedOut = _checkoutsService.IsCheckedOut(id),
                HoldCount = _checkoutsService.GetCurrentHolds(id).Count()
            };
            return View(model);
        }

        public IActionResult MarkLost(int assetId)
        {
            _checkoutsService.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult MarkFound(int assetId)
        {
            _checkoutsService.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkoutsService.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkoutsService.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }
    }
}
