using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroAvatarImageController : MonoBehaviour
{
    [SerializeField] private List<Sprite> heroAvatarSpriteList;
    [SerializeField] private Image heroAvatarImage;

    public void UpdateHeroImageAvatar ( PlayerConfig config )
    {
        heroAvatarImage.sprite = heroAvatarSpriteList[config.selectedHero];
    }
}




