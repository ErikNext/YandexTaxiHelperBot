using YandexTaxiHelperBot.Repository.Address;

namespace YandexTaxiHelperBot.Core.Services.AddressesService;

public class AddressesService
{
    private readonly IAddressesDatabase _addressesDatabase;

    public AddressesService(IAddressesDatabase addressesDatabase)
    {
        _addressesDatabase = addressesDatabase;
    }


    public async Task Create(AddressDbModel model)
    {
        var userAddresses = await GetUserAddresses(model.UserId);
        
        
        if(userAddresses.Any(x => 
               x.DestinationPoint.Equals(model.DestinationPoint) && x.DestinationPoint.Equals(model.DestinationPoint)))
            return;
        
        if(userAddresses.Any(x => 
               x.DestinationPoint == model.DestinationPoint && x.DeparturePoint == model.DeparturePoint))
            return;

        if (userAddresses.Count >= 3)
        {
            var oldAddress = userAddresses.OrderBy(x => x.CreatedDate).First();
            await Delete(oldAddress.UserId, oldAddress.Id);
        }
        await _addressesDatabase.Create(model);
    }

    public Task<List<AddressDbModel>> GetUserAddresses(string userId)
    {
        return _addressesDatabase.GetUserAddresses(userId);
    }

    public Task Delete(string userId, string addressId)
    {
        return _addressesDatabase.Delete(userId, addressId);
    }
}