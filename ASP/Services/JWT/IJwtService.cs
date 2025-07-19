namespace ASP.Services.JWT
{
    public interface IJwtService
    {
        String EncodeJwt(Object payload, Object? header = null, String? secret = null);
        (Object, Object) DecodeJwt(String jwt, String? secret = null);
    }
}
