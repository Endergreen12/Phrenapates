using Plana.Database;
using Plana.Parcel;
using Plana.FlatData;

public class ParcelService
{
    public static void AddOrConsumeWithParcel(AccountDB account, List<ParcelInfo> parcelInfos, bool doConsume = false)
    {
        foreach(var parcelInfo in parcelInfos)
        {
            switch(parcelInfo.Key.Type)
            {
                case ParcelType.Currency:
                    account.Currencies.First().CurrencyDict[(CurrencyTypes)parcelInfo.Key.Id] += parcelInfo.Amount * (doConsume ? -1 : 1);
                    break;
            }
        }
    }
}