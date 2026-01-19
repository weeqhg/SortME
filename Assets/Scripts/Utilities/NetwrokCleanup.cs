using UnityEngine;
using Unity.Netcode;

public class NetworkCleanup : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Принудительная сборка мусора
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        
        Debug.Log("Network cleanup completed");
    }
}