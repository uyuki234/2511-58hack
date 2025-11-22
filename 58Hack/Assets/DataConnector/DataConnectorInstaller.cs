using Common;
using UnityEngine;
using Zenject;

public class DataConnectorInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IDataReceiver>().To<DataConnector>().AsTransient();
    }
}