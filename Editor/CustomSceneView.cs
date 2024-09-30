/*
MIT License

Copyright (c) 2023 suzuryg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.aoyon.triangleselector
{
    public class CustomSceneView : SceneView
    {
        private static readonly Vector2 MIN_SIZE = new Vector2(650f, 400f);
        private static bool isMouseOver = false;
        private static SceneView _defaultSceneView;
        private static CustomSceneView _window = null;

        public static CustomSceneView ShowWindow(SceneView defaultSceneView)
        {
            _window = CreateWindow<CustomSceneView>();
            _window.titleContent = new GUIContent("Triangle Selector");
            _window.minSize = MIN_SIZE;
            _window.Show();
            _defaultSceneView = defaultSceneView;
            SetLastActiveSceneView(_defaultSceneView);
            InitializeCamera(_window);
            return _window;
        }

        private static void InitializeCamera(CustomSceneView customSceneView)
        {
            var copied = new CameraSettings();

            copied.speed = _defaultSceneView.cameraSettings.speed;
            copied.speedNormalized = _defaultSceneView.cameraSettings.speedNormalized;
            copied.speedMin = _defaultSceneView.cameraSettings.speedMin;
            copied.speedMax = _defaultSceneView.cameraSettings.speedMax;
            copied.easingEnabled = _defaultSceneView.cameraSettings.easingEnabled;
            copied.easingDuration = _defaultSceneView.cameraSettings.easingDuration;
            copied.accelerationEnabled = _defaultSceneView.cameraSettings.accelerationEnabled;
            copied.fieldOfView = _defaultSceneView.cameraSettings.fieldOfView;
            copied.nearClip = _defaultSceneView.cameraSettings.nearClip;
            copied.farClip = _defaultSceneView.cameraSettings.farClip;
            copied.dynamicClip = _defaultSceneView.cameraSettings.dynamicClip;
            copied.occlusionCulling = _defaultSceneView.cameraSettings.occlusionCulling;

            customSceneView.cameraSettings = copied;
        }

        public static void Dispose()
        {
            if (_window != null)
            {
                _window.Close();
            }
        }

        void OnDestroy()
        {
            _window = null;
            SetLastActiveSceneView(_defaultSceneView);
            isMouseOver = false;
            TriangleSelector.Dispose();
        }
    
        public void OnGUI()
        {
            if (mouseOverWindow == this)
            {
                if (!isMouseOver)
                {
                    isMouseOver = true;
                    //Debug.Log("OnFocus");
                    SetLastActiveSceneView(this);
                }
            }
            else
            {
                if (isMouseOver)
                {
                    isMouseOver = false;
                    //Debug.Log("OnUnFocus");
                    SetLastActiveSceneView(_defaultSceneView);
                }
            }
        }
        
        /*
        ref
        https://github.com/suzuryg/face-emo/blob/8ea4a835b0024437643a218086ea348d7c16a851/Packages/jp.suzuryg.face-emo/Editor/Detail/View/ExpressionEditor/ExpressionPreviewWindow.cs#L162-L173
        */
        private static void SetLastActiveSceneView(SceneView sceneView)
        {
            Type sceneViewType = typeof(SceneView);
            PropertyInfo lastActiveSceneViewInfo = sceneViewType.GetProperty("lastActiveSceneView", BindingFlags.Public | BindingFlags.Static);
            if (lastActiveSceneViewInfo != null)
            {
                lastActiveSceneViewInfo.SetValue(null, sceneView, null);
            }
            else
            {
                Debug.LogError("lastActiveSceneView property not found");
            }
        }

        public static bool IsSelected()
        {
            return isMouseOver;
        }

        public static void FocusCustomViewObject(SceneView sceneView, Mesh mesh, Transform origin)
        {
            Vector3 middleVertex = Vector3.zero;
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                middleVertex += origin.position + origin.rotation * vertices[i];
            }
            middleVertex /= vertices.Length;

            float cameraDistance = 0.3f;
            sceneView.LookAt(middleVertex, Quaternion.Euler(0, 180, 0), cameraDistance);
            sceneView.Repaint();
        }

        
    }
}