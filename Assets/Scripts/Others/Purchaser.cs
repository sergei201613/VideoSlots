using System;
using System.Collections.Generic;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.Purchasing;

public class Purchaser : MonoBehaviour, IStoreListener
{
    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    [Multiline]
    public string PublicKey = "Public key Google Play";

    public List<InforProducts> infomationProducts;
    public static Purchaser Instance;

    void Awake()
    {
        if (Instance) {
            //Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        List<string> lstStore = new List<string>();
        lstStore.Add(Store.GooglePlay.ToString());
        lstStore.Add(Store.AppleAppStore.ToString());
        lstStore.Add(Store.WinRT.ToString());
        lstStore.Add(Store.AmazonApps.ToString());
        lstStore.Add(Store.TizenStore.ToString());
        lstStore.Add(Store.SamsungApps.ToString());
        lstStore.Add(Store.FacebookStore.ToString());
        lstStore.Add(Store.XiaomiMiPay.ToString());
        lstStore.Add(Store.MoolahAppStore.ToString());
        lstStore.Add(Store.MacAppStore.ToString());
        
        //add product
        foreach (var item in infomationProducts)
        {
            if (item.ID != "")
            {
                IDs id = new IDs();
                if (item.DetailProductID != null && item.DetailProductID.Count != 0)
                {
                    List<string> copyStore = new List<string>();
                    copyStore = lstStore;
                    foreach (var store in item.DetailProductID)
                    {
                        id.Add(store.ID, store.Store.ToString());
                        for (int i = 0; i < copyStore.Count; i++)
                            if (copyStore[i].ToString() == store.Store.ToString())
                                copyStore[i] = "";
                    }

                    foreach (var it in copyStore)
                    {
                        if (it != "")
                            id.Add(item.ID, it.ToString());
                    }
                }
                else
                {
                    id.Add(item.ID, Store.GooglePlay.ToString());
                    id.Add(item.ID, Store.AppleAppStore.ToString());
                    id.Add(item.ID, Store.WinRT.ToString());
                    id.Add(item.ID, Store.AmazonApps.ToString());
                    id.Add(item.ID, Store.TizenStore.ToString());
                    id.Add(item.ID, Store.SamsungApps.ToString());
                    id.Add(item.ID, Store.FacebookStore.ToString());
                    id.Add(item.ID, Store.XiaomiMiPay.ToString());
                    id.Add(item.ID, Store.MoolahAppStore.ToString());
                    id.Add(item.ID, Store.MacAppStore.ToString());
                }
                builder.AddProduct(item.ID, item.ProductType, id);
            }
        }
        builder.Configure<IGooglePlayConfiguration>().SetPublicKey(PublicKey);
        UnityPurchasing.Initialize(this, builder);
    }


    public bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public string LocalizedPrice(string ID)
    {
        if (m_StoreController != null)
            return m_StoreController.products.WithID(ID).metadata.localizedPriceString;
        else
            return "";
    }

    public bool Purchased(string ID)
    {
        if (m_StoreController != null && ID != "")
            return m_StoreController.products.WithID(ID).hasReceipt;
        else return false;
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");

            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;

        ItemIAP[] itemIAPs = FindObjectsOfType<ItemIAP>();
        foreach (var it in itemIAPs)
            it.Start();
        //MenuManager.Instance.LoadGameList();
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        foreach (var item in infomationProducts)
        {
            if (String.Equals(args.purchasedProduct.definition.id, item.ID, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                print("Xu ly " + item.Modification + " " + item.Quantity);
                switch (item.Modification)
                {
                    case Modification.Coin:
                        if (item.ID == "com.slots.credit07")
                        {
                            PlayerPrefs.SetInt("soffer", 1);
                            if (Application.loadedLevelName != "Menu")
                                SlotMachine.Instance.specialOffer[0].SetTrigger("Close");
                        }

                        if (item.ID == "com.slots.credit08")
                        {
                            PlayerPrefs.SetInt("soffer", 2);
                            if (Application.loadedLevelName != "Menu")
                                SlotMachine.Instance.specialOffer[1].SetTrigger("Close");
                        }

                        if (Application.loadedLevelName == "Menu")
                            MenuManager.Instance.PlusMoney((ulong)item.Quantity);
                        else
                            SlotMachine.Instance.PlusMoney((ulong)item.Quantity);

                        PlayerPrefs.SetInt("totalSpin", -1);
                        PlayerPrefs.Save();

                        break;

                    case Modification.Hint:
                        if (Application.loadedLevelName == "Menu")
                            MenuManager.Instance.PlusMoney((ulong)item.Quantity);
                        else
                            SlotMachine.Instance.PlusMoney((ulong)item.Quantity);

                        int eg = 0;
                        if (item.ID == "com.slots.credit09")
                            eg += 2;

                        if (item.ID == "com.slots.credit10")
                            eg += 1;

                        if (item.ID == "com.slots.credit16")
                        {
                            eg += 3;
                            PlayerPrefs.SetInt("soffer", 3);
                        }

                        if (eg > 0 && Application.loadedLevelName == "Menu")
                            GoldEGG.Instance.OpenEGG(eg);


                        break;

                    case Modification.OpenLevel:
                        //MenuManager.Instance.LoadGameList();
                        break;
                }
            }
        }
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void jsOpenClick();

    [DllImport("__Internal")]
    private static extern void jsCloseClick();

    [DllImport("__Internal")]
    private static extern void jsRefreshClick();

    [DllImport("__Internal")]
    private static extern void jsBuyClick(float price);

    public void RefreshWebView()
    {
        jsOpenClick();
        jsRefreshClick();
    }

    public void OpenWebView()
    {
        jsOpenClick();
    }

    public void CloseWebView()
    {
        jsCloseClick();
    }

    public void BuyClick(float price)
    {
        jsBuyClick(price);
    }

    public void OnJsBuy(string ID)
    {
        foreach (var item in infomationProducts)
        {
            if (item.ID == ID)
            {
                print("Xu ly " + item.Modification + " " + item.Quantity);
                switch (item.Modification)
                {
                    case Modification.Coin:
                        MenuManager.Instance.PlusMoney((ulong)item.Quantity);
                        PlayerPrefs.SetInt("totalSpin", -1);
                        PlayerPrefs.Save();
                        break;

                    case Modification.Hint:
                        break;

                    case Modification.OpenLevel:
                        //MenuManager.Instance.LoadGameList();
                        break;
                }
            }
        }
    }
#endif
}

[System.Serializable]
public class InforProducts {
    public string Name;
    [Tooltip("If the empty data field is understood to be advertising")]
    public string ID;
    [Tooltip("Add all ID products if you want to customize the store. Emptying will initialize all stores and using default ID")]
    public List<DetailProdutcs> DetailProductID;
    public Sprite Sprite;
    public ProductType ProductType;
    [Multiline]
    public string Description;
    [Tooltip("Default price. Currency unit in USA dolla")]
    public float Price;
    public Modification Modification;
    public ulong Quantity;
}

[System.Serializable]
public class DetailProdutcs {
    public Store Store;
    public string ID;
}

[System.Serializable]
public enum Modification { 
    None,
    Hint,
    Coin,
    EXP,
    OpenLevel
}

public enum Store{
    GooglePlay,
    AppleAppStore,
    MacAppStore,
    WinRT,
    AmazonApps,
    TizenStore,
    SamsungApps,
    FacebookStore,
    XiaomiMiPay,
    MoolahAppStore
}