/*
MIT License

Copyright (c) 2022 anatawa12

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

using UnityEngine;
using UnityEditor;

namespace com.aoyon.triangleselector.utils
{
    public class CustomAnimationMode
    {
        public static void StartAnimationMode(SkinnedMeshRenderer rendrer)
        {
            StopAnimationMode();
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            try
            {
                var binding = EditorCurveBinding.PPtrCurve("", typeof(SkinnedMeshRenderer), "m_Mesh");
                var modification = new PropertyModification
                {
                    target = rendrer,
                    propertyPath = "m_Mesh",
                    objectReference = rendrer.sharedMesh
                };

                AnimationMode.AddPropertyModification(binding, modification, true);
            }
            finally
            {
                AnimationMode.EndSampling();
            }
        }

        public static void StopAnimationMode()
        {
            AnimationMode.StopAnimationMode();
        }
    }
}
