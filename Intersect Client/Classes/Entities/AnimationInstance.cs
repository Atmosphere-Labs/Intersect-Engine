﻿using System;
using Intersect.GameObjects;
using IntersectClientExtras.File_Management;
using IntersectClientExtras.GenericClasses;
using IntersectClientExtras.Graphics;
using Intersect_Client.Classes.Core;
using Intersect_Client.Classes.General;

namespace Intersect_Client.Classes.Entities
{
    public class AnimationInstance
    {
        private int _renderDir;
        private float _renderX;
        private float _renderY;
        public bool AutoRotate;
        public bool Hidden;
        private bool infiniteLoop;
        private int lowerFrame;
        private int lowerLoop;
        private long lowerTimer;
        private AnimationBase myBase;
        private bool showLower = true;
        private bool showUpper = true;
        private MapSound sound;
        private int upperFrame;
        private int upperLoop;
        private long upperTimer;
        private int ZDimension = -1;

        public AnimationInstance(AnimationBase animBase, bool loopForever, bool autoRotate = false, int zDimension = -1)
        {
            myBase = animBase;
            if (myBase != null)
            {
                lowerLoop = animBase.LowerAnimLoopCount;
                upperLoop = animBase.UpperAnimLoopCount;
                lowerTimer = Globals.System.GetTimeMS() + animBase.LowerAnimFrameSpeed;
                upperTimer = Globals.System.GetTimeMS() + animBase.UpperAnimFrameSpeed;
                infiniteLoop = loopForever;
                AutoRotate = autoRotate;
                ZDimension = zDimension;
                sound = GameAudio.AddMapSound(myBase.Sound, 0, 0, 0, loopForever, 12);
                lock (GameGraphics.AnimationLock)
                {
                    GameGraphics.LiveAnimations.Add(this);
                }
            }
            else
            {
                Dispose();
            }
        }

        public void Draw(bool upper = false)
        {
            if (Hidden) return;
            float rotationDegrees = 0f;
            if (AutoRotate)
            {
                switch (_renderDir)
                {
                    case 0: //Up
                        rotationDegrees = 0f;
                        break;
                    case 1: //Down
                        rotationDegrees = 180f;
                        break;
                    case 2: //Left
                        rotationDegrees = 270f;
                        break;
                    case 3: //Right
                        rotationDegrees = 90f;
                        break;
                    case 4: //NW
                        rotationDegrees = 315f;
                        break;
                    case 5: //NE
                        rotationDegrees = 45f;
                        break;
                    case 6: //SW
                        rotationDegrees = 225f;
                        break;
                    case 7: //SE
                        rotationDegrees = 135f;
                        break;
                }
            }

            if ((!upper && showLower && ZDimension < 1) || (!upper && showLower && ZDimension > 0))
            {
                //Draw Lower
                GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                    myBase.LowerAnimSprite);
                if (tex != null)
                {
                    if (myBase.LowerAnimXFrames > 0 && myBase.LowerAnimYFrames > 0)
                    {
                        int frameWidth = tex.GetWidth() / myBase.LowerAnimXFrames;
                        int frameHeight = tex.GetHeight() / myBase.LowerAnimYFrames;
                        GameGraphics.DrawGameTexture(tex,
                            new FloatRect((lowerFrame % myBase.LowerAnimXFrames) * frameWidth,
                                (float) Math.Floor((double) lowerFrame / myBase.LowerAnimXFrames) * frameHeight,
                                frameWidth,
                                frameHeight),
                            new FloatRect(_renderX - frameWidth / 2, _renderY - frameHeight / 2, frameWidth, frameHeight),
                            Intersect.Color.White, null, GameBlendModes.None, null, rotationDegrees);
                    }
                }
                int offsetX = myBase.LowerLights[lowerFrame].OffsetX;
                int offsetY = myBase.LowerLights[lowerFrame].OffsetY;
                var offset = RotatePoint(new Point((int)offsetX, (int)offsetY), new Point(0, 0), rotationDegrees + 180);
                GameGraphics.AddLight((int) _renderX - offset.X,
                    (int) _renderY - offset.Y, myBase.LowerLights[lowerFrame].Size,
                    myBase.LowerLights[lowerFrame].Intensity, myBase.LowerLights[lowerFrame].Expand,
                    myBase.LowerLights[lowerFrame].Color);
            }

            if ((upper && showUpper && ZDimension != 0) || (upper && showUpper && ZDimension == 0))
            {
                //Draw Upper
                GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                    myBase.UpperAnimSprite);
                if (tex != null)
                {
                    if (myBase.UpperAnimXFrames > 0 && myBase.UpperAnimYFrames > 0)
                    {
                        int frameWidth = tex.GetWidth() / myBase.UpperAnimXFrames;
                        int frameHeight = tex.GetHeight() / myBase.UpperAnimYFrames;

                        GameGraphics.DrawGameTexture(tex,
                            new FloatRect((upperFrame % myBase.UpperAnimXFrames) * frameWidth,
                                (float) Math.Floor((double) upperFrame / myBase.UpperAnimXFrames) * frameHeight,
                                frameWidth,
                                frameHeight),
                            new FloatRect(_renderX - frameWidth / 2, _renderY - frameHeight / 2, frameWidth, frameHeight),
                            Intersect.Color.White, null, GameBlendModes.None, null, rotationDegrees);
                    }
                }
                int offsetX = myBase.UpperLights[upperFrame].OffsetX;
                int offsetY = myBase.UpperLights[upperFrame].OffsetY;
                var offset = RotatePoint(new Point((int)offsetX, (int)offsetY), new Point(0, 0), rotationDegrees + 180);
                GameGraphics.AddLight((int) _renderX - offset.X,
                    (int) _renderY - offset.Y, myBase.UpperLights[upperFrame].Size,
                    myBase.UpperLights[upperFrame].Intensity, myBase.UpperLights[upperFrame].Expand,
                    myBase.UpperLights[upperFrame].Color);
            }
        }

        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                     sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                     cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public void Hide()
        {
            Hidden = true;
        }

        public void Show()
        {
            Hidden = false;
        }

        public void Dispose()
        {
            lock (GameGraphics.AnimationLock)
            {
                if (sound != null)
                {
                    sound.Stop();
                    sound = null;
                }
                GameGraphics.LiveAnimations.Remove(this);
            }
        }

        public void SetPosition(float worldX, float worldY, int mapx, int mapy, int map, int dir, int z = 0)
        {
            _renderX = worldX;
            _renderY = worldY;
            if (sound != null)
            {
                sound.UpdatePosition(mapx, mapy, map);
            }
            if (dir > -1) _renderDir = dir;
            ZDimension = z;
        }

        public void Update()
        {
            if (myBase != null)
            {
                if (sound != null)
                {
                    sound.Update();
                }
                if (lowerTimer < Globals.System.GetTimeMS() && showLower)
                {
                    lowerFrame++;
                    if (lowerFrame >= myBase.LowerAnimFrameCount)
                    {
                        lowerLoop--;
                        lowerFrame = 0;
                        if (lowerLoop < 0)
                        {
                            if (infiniteLoop)
                            {
                                lowerLoop = myBase.LowerAnimLoopCount;
                            }
                            else
                            {
                                showLower = false;
                            }
                        }
                    }
                    lowerTimer = Globals.System.GetTimeMS() + myBase.LowerAnimFrameSpeed;
                }
                if (upperTimer < Globals.System.GetTimeMS() && showUpper)
                {
                    upperFrame++;
                    if (upperFrame >= myBase.UpperAnimFrameCount)
                    {
                        upperLoop--;
                        upperFrame = 0;
                        if (upperLoop < 0)
                        {
                            if (infiniteLoop)
                            {
                                upperLoop = myBase.UpperAnimLoopCount;
                            }
                            else
                            {
                                showUpper = false;
                            }
                        }
                    }
                    upperTimer = Globals.System.GetTimeMS() + myBase.UpperAnimFrameSpeed;
                }
                if (!showLower && !showUpper)
                {
                    Dispose();
                }
            }
        }

        public void SetDir(int dir)
        {
            _renderDir = dir;
        }
    }
}