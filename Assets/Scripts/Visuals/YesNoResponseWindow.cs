using System;
using UnityEngine;

public class YesNoResponseWindow : MonoBehaviour{

    [SerializeField] private GameObject Layout;
    
    void Start(){
        YSWindowInteraction.YSWindowOpen += YSWindowInteraction_YSwindowOpen;
        YSWindowInteraction.YSWindowClose += YSWindowInteraction_YSWindowClose;
    }

    private void YSWindowInteraction_YSWindowClose(object sender, EventArgs e) {
        Layout.SetActive(false);
    }

    private void YSWindowInteraction_YSwindowOpen(object sender, EventArgs e) {
        Layout.SetActive(true);
    }
}
