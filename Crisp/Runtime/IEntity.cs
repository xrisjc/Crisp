namespace Crisp.Runtime
{
    interface IEntity
    {
        bool GetAttribute(string name, out dynamic value);
        bool SetAttribute(string name, dynamic value);
        bool GetMethod(string name, out Function method);
    }
}
