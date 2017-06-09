namespace Kumquat.SAS.SC.Interfaces
{
    public interface IConverterFactory
    {
        IConverter CreateNew(string name);
    }
}
