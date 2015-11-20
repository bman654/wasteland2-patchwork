using System;
using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;

namespace W2PWMod.Mods.FreeCamera
{
    [NewType]
    public static class FreeCameraOptions
    {
        public static bool FarEnabled => W2PWMod.Options.GetBool("Free Camera Zoomed Out Angle Enabled");
        public static bool NearEnabled => W2PWMod.Options.GetBool("Free Camera Zoomed In Angle Enabled");
    }

    [ModifiesType]
    public class mod_Drama : Drama
    {
        [ModifiesMember("DoCameraFocus")]
        public void DoCameraFocusNew(Vector3 targetPosition)
        {
            // ensure we zoom no closer than the original game's minimum zoom
            var maxZoom = FreeCameraOptions.NearEnabled ? 0.7f : 1f;
            this.Action_FocusCamera(targetPosition, Mathf.Min(maxZoom, this.cameraZoom), Camera.main.transform.forward, this.cameraSpeedOverride, this.cameraSnapInstant, 1f);
        }
    }

    [ModifiesType]
    public class mod_InXileSplineInterpolator: InXileSplineInterpolator
    {
        [NewMember]
        [DuplicatesBody("GetSplineNodes")]
        public CameraSplineNode[] GetSplineNodesOriginal()
        {
            return null;
        }

        [ModifiesMember("GetSplineNodes")]
        public CameraSplineNode[] GetSplineNodesNew()
        {
            var result = GetSplineNodesOriginal();

            if (!FreeCameraOptions.FarEnabled && !FreeCameraOptions.NearEnabled)
            {
                return result;
            }

#if false
            for (var i = 0; i < result.Length; ++i)
            {
                var node = result[i];
                Helper.W2ModDebug.Log("node before {0}:{6} time={1} fov={2} rotation={3} euler={5} {8} position={4} {7}", i, node.time, node.fov, node.transform.rotation, node.transform.position, node.transform.rotation.eulerAngles, node.name, node.transform.localPosition, node.transform.localRotation.eulerAngles);
            }
#endif
            if (result != null && result.Length == 3)
            {
                Helper.W2ModDebug.Log("adjusting camera spline");

                /* Add a node between the middle node and the minimum node, as a clone of the current minimum zoom
                 * Adjust the minimum zoom to have an almost horizontal angle and really close to the target
                 * Adjust the maximum zoom to have a bit more of an angle to it than the default
                 * Adjust the new node to be like the original minimum zoom and set its time very close to the minimum zoom (0.10)
                 * Adjust the old middle node to be in between the maximum and the new node
                 */
                var newResult = new CameraSplineNode[4];
                newResult[0] = result[0];
                newResult[1] = GameObject.Instantiate(result[0]);
                newResult[2] = result[1];
                newResult[3] = result[2];
                newResult[0].name = "1";
                newResult[1].name = "2";
                newResult[2].name = "3";
                newResult[3].name = "4";

                result = newResult;

                if (FreeCameraOptions.NearEnabled)
                {
                    result[0].transform.localRotation = Quaternion.Euler(10.0f, 0.0f, 0.0f);
                }
                else
                {
                    result[0].transform.localRotation = Quaternion.Euler(36.0f, 0.0f, 0.0f);
                }

                result[1].transform.localRotation = Quaternion.Euler(36.0f, 0.0f, 0.0f);

                if (FreeCameraOptions.FarEnabled)
                {
                    result[2].transform.localRotation = Quaternion.Euler(46.0f, 0.0f, 0.0f); // original: (53.0, 0.0, 0.0)
                    result[3].transform.localRotation = Quaternion.Euler(53.0f, 0.0f, 0.0f); // original: (67.0, 0.0, 0.0)
                }
                else
                {
                    result[2].transform.localRotation = Quaternion.Euler(53.0f, 0.0f, 0.0f); // original: (53.0, 0.0, 0.0)
                    result[3].transform.localRotation = Quaternion.Euler(67.0f, 0.0f, 0.0f); // original: (67.0, 0.0, 0.0)
                }

                /* y is height above the ground
                 * y = 0 puts camera a few feet above the head
                 *
                 * z is lateral distance away from target
                 * z = 0 puts camera a few feed behind the target
                 */

                if (FreeCameraOptions.NearEnabled)
                {
                    result[0].transform.localPosition = new Vector3(0f, -1.0f, 0.0f); // original: (0.0,  8.0, -10.0)
                }
                else
                {
                    result[0].transform.localPosition = new Vector3(0f, 8.0f, -10.0f); // original: (0.0,  8.0, -10.0)
                }
                result[1].transform.localPosition = new Vector3(0f,  8.0f, -10.0f);

                if (FreeCameraOptions.FarEnabled)
                {
                    result[2].transform.localPosition = new Vector3(0f, 23.7f, -14.2f); // original: (0.0, 23.7, -14.2)
                    result[3].transform.localPosition = new Vector3(0f, 43.7f, -22.0f); // original: (0.0, 43.7, -14.2)
                }
                else
                {
                    result[2].transform.localPosition = new Vector3(0f, 23.7f, -14.2f); // original: (0.0, 23.7, -14.2)
                    result[3].transform.localPosition = new Vector3(0f, 43.7f, -14.2f); // original: (0.0, 43.7, -14.2)
                }

                result[0].time = 0.00f;
                result[1].time = FreeCameraOptions.NearEnabled ? 0.10f : 0.01f;
                result[2].time = 0.50f;
                result[3].time = 1.00f;

                result[1].transform.SetParent(result[0].transform.parent, false);
            }

#if false
            for (var i = 0; i < result.Length; ++i)
            {
                var node = result[i];
                Helper.W2ModDebug.Log("node after {0}:{6} time={1} fov={2} rotation={3} euler={5} {8} position={4} {7}", i, node.time, node.fov, node.transform.rotation, node.transform.position, node.transform.rotation.eulerAngles, node.name, node.transform.localPosition, node.transform.localRotation.eulerAngles);
            }
#endif
            return result;
        }
    }
}
