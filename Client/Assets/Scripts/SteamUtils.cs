using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

namespace ProjectRedfield.SteamUtils
{
    public static class SteamUtils
    {
        public static Sprite GetMediumFriendAvatar(CSteamID steamId)
        {
            int ret = SteamFriends.GetMediumFriendAvatar(steamId);
            var medAvatar = SteamUtils.GetSteamImageAsTexture2D(ret);
			return medAvatar;
        }

		public static Sprite GetSteamImageAsTexture2D(int iImage)
		{
			Texture2D ret = null;
			uint ImageWidth;
			uint ImageHeight;
			bool bIsValid = Steamworks.SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

			if (bIsValid)
			{
				byte[] Image = new byte[ImageWidth * ImageHeight * 4];

				bIsValid = Steamworks.SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
				if (bIsValid)
				{
					ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
					ret.LoadRawTextureData(Image);
					ret.Apply();
				}
			}

			return Sprite.Create(ret, new Rect(0, 0, ImageWidth, ImageHeight), new Vector2());
		}

	}

}