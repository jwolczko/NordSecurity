namespace partycli.Application.Interfaces;

public interface ISettingsStorage
{
    string GetValue(string name);

    void SetValue(string name, string value);
}
