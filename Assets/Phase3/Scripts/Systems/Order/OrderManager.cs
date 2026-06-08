using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private OrderData[] orders;
    [SerializeField] private int currentIndex;

    public OrderData CurrentOrder { get; private set; }

    private void Awake()
    {
        RefreshCurrentOrder();
    }

    private void OnValidate()
    {
        RefreshCurrentOrder();
    }

    public OrderData GetCurrentOrder()
    {
        RefreshCurrentOrder();
        return CurrentOrder;
    }

    public void NextOrder()
    {
        if (orders == null || orders.Length == 0) return;
        currentIndex = (currentIndex + 1) % orders.Length;
        RefreshCurrentOrder();
    }

    private void RefreshCurrentOrder()
    {
        if (orders == null || orders.Length == 0)
        {
            CurrentOrder = null;
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, orders.Length - 1);
        CurrentOrder = orders[currentIndex];
    }
}
