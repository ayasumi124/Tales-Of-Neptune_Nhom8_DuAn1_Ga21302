using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class HeartHeart : MonoBehaviour
{
    public Sprite fullHeart, halfHeart, emptyHeart;

    public void SetHeartStatus(HeartStatus status)
    {
        switch (status)
        {
            case HeartStatus.Full:
                heartImage.sprite = fullHeart;
                break;
            case HeartStatus.Half:
                heartImage.sprite = halfHeart;
                break;
            case HeartStatus.Empty:
                heartImage.sprite = emptyHeart;
                break;
        }
    }
    Image heartImage;

    void Awake()
    {
        heartImage = GetComponent<Image>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum HeartStatus
{
    Empty =0,

    Half =1, 
    Full =2,
}