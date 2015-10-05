README v0.95 - 26.06.14

AL_UMAMaleCivilianPack is a UMA male cloth pack by Alien Lab. 
Some knowlegde of UMA (Unity Multipurpose Avatar) is higly recomended before using this pack :)

Right now it contains 11 fbx garment models and 23 texture overlays in UMA format:
-Torso
--Long Sleeve Shirt
---grey version
---blue checkered pattern
--Sweatshirt
---grey version
---2color variation
--TShirt
---grey version
--Hoodie (hood down version)
---grey version
---2color variation
-Legs
--Jeans
---grey version
---blue jeans
--Cargo Pants
---grey version
---woodland camo
---wornout blue grey
--Suit Trousers
---grey version
---2color variation
--Shorts
---grey version
---checker pattern
-Shoes
--Smart Shoes
---dark grey version
---brown version
--Sneakers
---grey version
---red version
--Hiking Shoes
---grey version
---2 color version


!!!IMPORTANT!!!
On the users request we added legacy versions of the slots (ones with 'Legacy' suffix)
Legacy mode is simpler to start with as geometry is modelled *over* UMA body, so one can just put cloth on the UMA avatar. Unfortunately since there is no hidden vertices removal algorithm hidden body parts under the cloths are still processed as it can be seen on the wireframe view. Additionally it gives a bit bulkier look.

However recommended way is to use default models, which work in conjunction with special body replacements slots ('alt' for 'alternative'), allowing better geometry mixing for a male body. In the example scene you can find how we use replacement geometry using 'requirement' parameter in crowd definition file.

Replacements table:
- TShirt -> AL_MaleAltHandsLong
- Shorts -> AL_MaleAltLegsLong
- Jeans, Suit Trousers, Cargo Pants -> AL_MaleAltLegsShort
- Long Sleeve Shirt, Sweatshirt, Hoodie, TShirt -> AL_MaleAltTorsoFix 


REQUIREMENTS: 
UMA - Unity Multipurpose Avatar pack, available freely here: https://www.assetstore.unity3d.com/en/#!/content/13930
Pack works on both Unity Pro and Unity, although there is major difference in creating characters performance (no direct access to texture memory in a free version - http://unity3d.com/unity/licenses)

INSTALLATION:
After downloading and importing the UMA to fresh project (latest compatible and verified version is 1.1.0.0 / 24.03.14 release) import this pack.

EXAMPLE SCENES:
!!!!Warning again! Example scenes won't work without UMA installed! :)!!!

location:  Assets\UMA\UMA_Content\AlienLab\ExampleScenes\Scene01 - AL UMA crowd Legacy.unity
Legacy mode example - as you can see in wireframe view there is full uma body under the clothes, using AL_HumanMale_Legacy random set . Contains fully randomized monochrome overlays, default monochrome, and default color overlays.
Currently there are 1 extra random sets available under path Assets\UMA\UMA_Content\AlienLab\ExampleScenes\RandomSets
- AL_HumanMale_LegacyMono - only monochrome base overlays

location:  Assets\UMA\UMA_Content\AlienLab\ExampleScenes\Scene01 - AL UMA crowd.unity
Standard mode example. Using AL_HumanMale_Full random set. Contains fully randomized monochrome overlays, default monochrome, and default color overlays

Additionally you can change the random sets manually changing Random Pool settings using the "UMA/UMA Crowd" object in the scene.
Currently there are 2 extra random sets available under path Assets\UMA\UMA_Content\AlienLab\ExampleScenes\RandomSets

- AL_HumanMale_Mono - only monochrome base overlays
- AL_HumanMale_ColorizedMono - monochrome overlays colorized with the UMA color randomization.

RandomSets are provided as learning examples :)

-----------------------------
Unity thread:
http://forum.unity3d.com/threads/al-male-civilians-pack-for-uma.248540/


