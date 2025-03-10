using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClickPlayer : MonoBehaviour
{
  public void Click()
  {
    AudioManager.instance.PlayClick();
  }
}
