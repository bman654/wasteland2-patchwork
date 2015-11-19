using System;
using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random=UnityEngine.Random;


namespace W2PWMod.Mods.FreeCombatCamera
{
    public class mod_Option
    {
        public static bool Enabled { get; set; }
    }

    [ModifiesType]
    public class mod_AIAction_CameraSnapAndFollow : AIAction_CameraSnapAndFollow
    {
        [MemberAlias("Activate", typeof(AIAction), AliasCallMode.NonVirtual)]
        public void BaseActivate()
        {
            // A call to this method will be translated to a "base.Activate()" call in the target assembly.
        }

        [NewMember]
        [DuplicatesBody("Activate")]
        public void ActivateOld()
        {
            // -- contents replaced by original game function --
        }

        [ModifiesMember("Activate")]
        public void ActivateNew()
        {
            if (mod_Option.Enabled)
            {
                BaseActivate();
            }
            else
            {
                ActivateOld();
            }
        }

        [NewMember]
        [DuplicatesBody("IsComplete")]
        public bool IsCompleteOld()
        {
            // -- contents replaced by original game function --
            return true;
        }

        [ModifiesMember("IsComplete")]
        public bool IsCompleteNew()
        {
            return mod_Option.Enabled || IsCompleteOld();
        }
    }

    [ModifiesType]
    public class mod_AIAction_WaitForCamera : AIAction_WaitForCamera
    {
        [MemberAlias("Activate", typeof(AIAction), AliasCallMode.NonVirtual)]
        public void BaseActivate()
        {
            // A call to this method will be translated to a "base.Activate()" call in the target assembly.
        }

        [NewMember]
        [DuplicatesBody("Activate")]
        public void ActivateOld()
        {
            // -- contents replaced by original game function --
        }

        [ModifiesMember("Activate")]
        public void ActivateNew()
        {
            if (mod_Option.Enabled)
            {
                BaseActivate();
            }
            else
            {
                ActivateOld();
            }
        }

        [NewMember]
        [DuplicatesBody("IsComplete")]
        public bool IsCompleteOld()
        {
            // -- contents replaced by original game function --
            return true;
        }

        [ModifiesMember("IsComplete")]
        public bool IsCompleteNew()
        {
            return mod_Option.Enabled || IsCompleteOld();
        }
    }

    [ModifiesType]
    public class mod_CameraControllerSpline : CameraControllerSpline
    {
        [NewMember]
        public bool IgnoreFollowMob;

        [NewMember]
        [DuplicatesBody("Follow")]
        public void FollowOld(Mob m)
        {
            // -- contents replaced by original game function --
        }

        [ModifiesMember("Follow")]
        public void FollowNew(Mob m)
        {
            if (!IgnoreFollowMob)
            {
                FollowOld(m);
            }
        }

        [ModifiesMember("Update")]
        public void UpdateNew()
        {
            if (this.coroutineControl || Camera.main == null)
            {
                return;
            }
            InputManager instance = MonoBehaviourSingleton<InputManager>.GetInstance(false);
            CombatManager instance2 = MonoBehaviourSingleton<CombatManager>.GetInstance(false);
            if ((this.followingMob || this.followingObject) && (instance.cameraMove.x != 0f || instance.cameraMove.y != 0f) && (mod_Option.Enabled || !instance2.inCombat || instance2.isPlayersTurn)) // MODIFIED
            {
                this.followingMob = false;
                this.followingObject = false;
                this.timeStart = 0f;
            }
            if (!MonoBehaviourSingleton<InputManager>.GetInstance(false).isGamepadOn && this.centerOnPc != null && (instance.cameraMove.x != 0f || instance.cameraMove.y != 0f))
            {
                this.centerOnPc = null;
                this.snapping = false;
            }
            else
            {
                if (instance2.inCombat && this.centerInCombat && (instance.cameraMove.x != 0f || instance.cameraMove.y != 0f))
                {
                    this.centerInCombat = false;
                    this.snapping = false;
                }
            }
            if (this.centerOnPc != null && this.snapping && !instance2.inCombat && !Drama.isCutsceneOn)
            {
                this.snapToPosition = this.FindSnapPositionToCenterObject(this.centerOnPc.transform.position, this.centerOnPcView);
            }
            else
            {
                if (instance2.inCombat && this.centerInCombat && this.snapping)
                {
                    this.snapToPosition = this.FindSnapPositionToCenterObject(this.combatCenterPoint, 0.5f);
                }
            }
            if (this.isShaking && Time.time < this.shakeEndTime)
            {
                Vector3 position = this.shakeOrigin + Random.onUnitSphere * this.shakeAmplitude;
                if (Time.time - this.shakeLastFrequencyTime >= this.shakeFrequencyDelay)
                {
                    this.shakeLastFrequencyTime = Time.time;
                    base.transform.position = position;
                    this.shakeAmplitude = Mathf.Max(0f, this.shakeAmplitude - this.shakeDecay);
                }
            }
            else
            {
                if (this.isShaking)
                {
                    base.transform.position = this.shakeOrigin;
                    this.isShaking = false;
                }
                else
                {
                    if (this.snapping)
                    {
                        AnimationCurve animationCurve;
                        if (this.useCharacterSnap)
                        {
                            animationCurve = this.characterSnapAnimationCurve;
                        }
                        else
                        {
                            if (instance2.inCombat)
                            {
                                if (instance.gamepadMenuMode == GamepadMenuMode.none && instance2.isPlayersTurn && instance2.GetCurrentMob() != null && (instance2.GetCurrentMob() as PC).combatActionState == Mob.CombatActionState.THINKING)
                                {
                                    animationCurve = this.combatMoveCursorAnimationCurve;
                                }
                                else
                                {
                                    animationCurve = this.combatSnapAnimationCurve;
                                }
                            }
                            else
                            {
                                animationCurve = this.snapAnimationCurve;
                            }
                        }
                        float time = animationCurve[animationCurve.length - 1].time;
                        float time2 = Mathf.Clamp(Time.time - this.snapStartTime, 0f, time);
                        float num = animationCurve.Evaluate(time2);
                        float t = num / animationCurve[animationCurve.length - 1].value;
                        base.transform.position = Vector3.Lerp(this.snapFromPosition, this.snapToPosition, t);
                        if (this.snappingZoom)
                        {
                            this.currentZoom = Mathf.Lerp(this.snapFromZoom, this.snapToZoom, t);
                        }
                        if (this.snappingRotation)
                        {
                            Vector3 forward = Vector3.Lerp(this.snapFromForward, this.snapToForward, t);
                            base.transform.rotation = Quaternion.LookRotation(forward);
                        }
                        if (Time.time - this.snapStartTime >= time)
                        {
                            this.snapping = false;
                        }
                    }
                    else
                    {
                        if (Time.time >= this.timeStart)
                        {
                            if (!instance2.inCombat && this.centerOnPc != null && !Drama.isCutsceneOn)
                            {
                                if (!this.snapping)
                                {
                                    Vector3 zero = Vector3.zero;
                                    base.transform.position = Vector3.SmoothDamp(base.transform.position, this.FindSnapPositionToCenterObject(this.centerOnPc.transform.position, this.centerOnPcView), ref zero, 0.1f);
                                }
                            }
                            else
                            {
                                if (instance2.inCombat && this.centerInCombat && !Drama.isCutsceneOn)
                                {
                                    if (!this.snapping)
                                    {
                                        Vector3 zero2 = Vector3.zero;
                                        base.transform.position = Vector3.SmoothDamp(base.transform.position, this.FindSnapPositionToCenterObject(this.combatCenterPoint, 0.5f), ref zero2, 0.1f);
                                    }
                                }
                                else
                                {
                                    if (this.followingMob)
                                    {
                                        if (this.followMob == null || this.followMob.mobState == Mob.MobState.DEAD || (!(this.followMob.ai.FindCurrentActionInChildren() is AIAction_Move) && !(this.followMob.ai.FindCurrentActionInChildren() is AIAction_Path)))
                                        {
                                            this.followingMob = false;
                                            this.followMob = null;
                                        }
                                        else
                                        {
                                            Vector3 vector = this.followMob.transform.position;
                                            if (this.followLookAheadMin > 0f && this.followMob.navMeshAgent.hasPath)
                                            {
                                                float y = Camera.main.transform.localPosition.y;
                                                Vector3 b = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, y));
                                                Vector3 a = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, y));
                                                float num2 = Mathf.Max(this.followLookAheadMin, Vector3.Distance(a, b) * 0.9f);
                                                num2 *= Mathf.Clamp01(this.followAcceleration.Evaluate(Time.time - this.timeStart));
                                                if (this.followMob.navMeshAgent.remainingDistance <= num2)
                                                {
                                                    vector = this.FindSnapPositionToCenterObject(this.followMob.navMeshAgent.destination, 0.5f);
                                                }
                                                else
                                                {
                                                    for (int i = 0; i < this.followMob.navMeshAgent.path.corners.Length - 1; i++)
                                                    {
                                                        float num3 = Vector3.Distance(this.followMob.navMeshAgent.path.corners[i], this.followMob.navMeshAgent.path.corners[i + 1]);
                                                        if (num3 >= num2)
                                                        {
                                                            vector = this.followMob.navMeshAgent.path.corners[i] + Vector3.Normalize(this.followMob.navMeshAgent.path.corners[i + 1] - this.followMob.navMeshAgent.path.corners[i]) * num2;
                                                            vector = this.FindSnapPositionToCenterObject(vector, 0.5f);
                                                            break;
                                                        }
                                                        num2 -= num3;
                                                    }
                                                }
                                            }
                                            vector.y = base.transform.position.y;
                                            base.transform.position = Vector3.Lerp(base.transform.position, vector, this.followAcceleration.Evaluate(Time.time - this.timeStart) * Time.deltaTime);
                                        }
                                    }
                                    else
                                    {
                                        if (this.followingObject)
                                        {
                                            if (this.followObj == null)
                                            {
                                                this.followingObject = false;
                                                this.followObj = null;
                                            }
                                            else
                                            {
                                                Vector3 position2 = this.followObj.transform.position;
                                                position2.y = base.transform.position.y;
                                                base.transform.position = Vector3.Lerp(base.transform.position, position2, this.followAcceleration.Evaluate(Time.time - this.timeStart) * Time.deltaTime);
                                            }
                                        }
                                        else
                                        {
                                            if (/*!instance2.inCombat || instance2.isPlayersTurn*/true) // MODIFIED
                                            {
                                                if (!this.snapping && (instance.cameraMove.x != 0f || instance.cameraMove.y != 0f))
                                                {
                                                    this.xtrans = Mathf.Lerp(this.xtrans, this.cameraSpeed * instance.cameraMove.x, this.acceleration.x * Time.deltaTime);
                                                    this.ztrans = Mathf.Lerp(this.ztrans, this.cameraSpeed * instance.cameraMove.y, this.acceleration.y * Time.deltaTime);
                                                    base.transform.Translate(this.xtrans * Time.deltaTime, 0f, this.ztrans * Time.deltaTime);
                                                }
                                                else
                                                {
                                                    this.xtrans = Mathf.Lerp(this.xtrans, 0f, this.deceleration.x * Time.deltaTime);
                                                    this.ztrans = Mathf.Lerp(this.ztrans, 0f, this.deceleration.y * Time.deltaTime);
                                                    if (Mathf.Abs(this.xtrans) < 0.1f)
                                                    {
                                                        this.xtrans = 0f;
                                                    }
                                                    if (Mathf.Abs(this.ztrans) < 0.1f)
                                                    {
                                                        this.ztrans = 0f;
                                                    }
                                                    base.transform.Translate(this.xtrans * Time.deltaTime, 0f, this.ztrans * Time.deltaTime);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (this.heightfield != null)
            {
                float heightSamplef = this.heightfield.GetHeightSamplef(base.transform.position.x, base.transform.position.z);
                base.transform.position = new Vector3(base.transform.position.x, heightSamplef, base.transform.position.z);
            }
            if (!instance.IsZoomInputFrozen())
            {
                float num4 = 0f;
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                if (instance.isGamepadOn)
                {
                    if (Mathf.Abs(cInput.GetAxis("Right Thumbstick Vert")) > 0.9f)
                    {
                        num4 = cInput.GetAxis("Right Thumbstick Vert") / (float)this.stickZoomSpeed;
                    }
                }
                else
                {
                    if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                    {
                        num4 = Input.GetAxis("Mouse ScrollWheel");
                    }
                    else
                    {
                        if (cInput.GetAxis("Zoom Axis") != 0f && realtimeSinceStartup - this.lastScrollTime > 0.05f)
                        {
                            this.lastScrollTime = realtimeSinceStartup;
                            num4 = ((cInput.GetAxis("Zoom Axis") <= 0f) ? -0.1f : 0.1f);
                        }
                    }
                }
                if (num4 != 0f)
                {
                    if (!this.zooming)
                    {
                        this.zooming = true;
                        this.zoomFrom = this.currentZoom;
                        this.zoomTo = this.zoomFrom + this.zoomSpeed * num4;
                        this.zoomTo = Mathf.Clamp(this.zoomTo, 0f, 1f);
                    }
                    else
                    {
                        this.zoomFrom = this.currentZoom;
                        this.zoomTo += this.zoomSpeed * num4;
                        this.zoomTo = Mathf.Clamp(this.zoomTo, 0f, 1f);
                    }
                    this.restoreZoom = false;
                    this.originalZoom = -1f;
                    this.snappingZoom = false;
                }
            }
            if (this.zooming)
            {
                float num5 = this.zoomTo - this.currentZoom;
                float num6 = num5 / (this.zoomTo - this.zoomFrom);
                if (float.IsNaN(num6))
                {
                    num6 = 0f;
                }
                float num7 = 1f - num6;
                num7 = Mathf.Clamp(num7, this.zoomSpeedCurve[0].time, this.zoomSpeedCurve[this.zoomSpeedCurve.length - 1].time);
                float num8 = this.zoomSpeedCurve.Evaluate(num7) * Mathf.Abs(this.zoomTo - this.zoomFrom);
                float num9 = num8 * Time.deltaTime;
                num9 = Mathf.Min(num9, Mathf.Abs(num5));
                num9 *= Mathf.Sign(num5);
                this.currentZoom += num9;
                if (Mathf.Approximately(this.currentZoom, this.zoomTo))
                {
                    this.zooming = false;
                }
            }
            if (this.restoreZoom)
            {
                this.currentZoom += this.zoomSpeed * Time.deltaTime;
                this.currentZoom = Mathf.Clamp(this.currentZoom, 0f, this.originalZoom);
                if (this.currentZoom >= this.originalZoom)
                {
                    this.restoreZoom = false;
                    this.originalZoom = -1f;
                }
            }
            InXileSplineInterpolator currentSplineInterpolator = this.GetCurrentSplineInterpolator();
            this.cameraObj.transform.position = currentSplineInterpolator.GetHermiteAtTime(1f - this.currentZoom);
            this.cameraScript.fieldOfView = currentSplineInterpolator.GetFovAtTime(1f - this.currentZoom);
            this.cameraObjTextBubbles.GetComponent<Camera>().fieldOfView = this.cameraScript.fieldOfView;
            bool flag = instance.cameraMove.z != 0f;
            if (flag)
            {
                base.transform.Rotate(new Vector3(0f, instance.cameraMove.z * this.rotateSpeed, 0f));
                this.snappingRotation = false;
            }
            if (instance.cameraMove.w != 0f && !this.rotating && !this.snapping)
            {
                this.rotateFrom = base.transform.rotation;
                this.rotateTo = this.rotateFrom * Quaternion.AngleAxis(45f * instance.cameraMove.w, Vector3.up);
                this.rotating = true;
                this.rotateStartTime = Time.time;
            }
            if (this.rotating)
            {
                float time3 = this.rotateAnimationCurve[this.rotateAnimationCurve.length - 1].time;
                float time4 = Mathf.Clamp(Time.time - this.rotateStartTime, 0f, time3);
                float num10 = this.rotateAnimationCurve.Evaluate(time4);
                float t2 = num10 / this.rotateAnimationCurve[this.rotateAnimationCurve.length - 1].value;
                base.transform.rotation = Quaternion.Lerp(this.rotateFrom, this.rotateTo, t2);
                if (Time.time - this.rotateStartTime >= time3)
                {
                    this.rotating = false;
                }
            }
            if (this.levelInfo != null && !this.coroutineControl)
            {
                Vector3 vector2 = base.transform.position;
                if (((this.centerOnPc != null && !instance2.inCombat) || (instance2.inCombat && this.centerInCombat)) && !Drama.isCutsceneOn && !this.snapping)
                {
                    Vector3 position3;
                    if (instance2.inCombat)
                    {
                        position3 = this.combatCenterPoint;
                    }
                    else
                    {
                        position3 = this.centerOnPc.transform.position;
                    }
                    vector2 = this.FindSnapPositionToCenterObject(position3, this.centerOnPcView);
                    this.levelInfo.bounds.Clamp(ref vector2);
                    Vector3 zero3 = Vector3.zero;
                    base.transform.position = Vector3.SmoothDamp(base.transform.position, vector2, ref zero3, 0.1f);
                    if (this.heightfield != null)
                    {
                        float heightSamplef2 = this.heightfield.GetHeightSamplef(base.transform.position.x, base.transform.position.z);
                        base.transform.position = new Vector3(base.transform.position.x, heightSamplef2, base.transform.position.z);
                    }
                }
                else
                {
                    this.levelInfo.bounds.Clamp(ref vector2);
                    base.transform.position = vector2;
                }
            }
            this.cameraObj.transform.rotation = currentSplineInterpolator.GetSquadAtTime(1f - this.currentZoom);
        }
    }

    [ModifiesType]
    public class mod_AIBehaviour_NPCCombat : AIBehaviour_NPCCombat
    {
        [NewMember]
        [DuplicatesBody("MoveTowardsTarget")]
        public bool MoveTowardsTargetOld(Mob target, bool meleeGetCloser, Dictionary<CombatAStarNode, int> positionMap)
        {
            // -- contents replaced by original game function --
            return false;
        }

        [ModifiesMember("MoveTowardsTarget")]
        public bool MoveTowardsTargetNew(Mob target, bool meleeGetCloser, Dictionary<CombatAStarNode, int> positionMap)
        {
            // suppress calls to Follow a mob while executing this function.
            // we could have modified the function, but this way is more future proof
            if (mod_Option.Enabled)
            {
                ((mod_CameraControllerSpline) MonoBehaviourSingleton<Game>.GetInstance(false).cameraController)
                    .IgnoreFollowMob = true;
            }

            var result = MoveTowardsTargetOld(target, meleeGetCloser, positionMap);

            if (mod_Option.Enabled)
            {
                ((mod_CameraControllerSpline) MonoBehaviourSingleton<Game>.GetInstance(false).cameraController)
                    .IgnoreFollowMob = false;
            }

            return result;
        }

        [NewMember]
        [DuplicatesBody("MoveOnPath")]
        public void MoveOnPathOld(Vector3[] path, int pathCost)
        {
            // -- contents replaced by original game function --
        }


        [ModifiesMember("MoveOnPath")]
        public void MoveOnPathNew(Vector3[] path, int pathCost)
        {
            // suppress calls to Follow a mob while executing this function.
            // we could have modified the function, but this way is more future proof
            if (mod_Option.Enabled)
            {
                ((mod_CameraControllerSpline)MonoBehaviourSingleton<Game>.GetInstance(false).cameraController)
                    .IgnoreFollowMob = true;
            }

            MoveOnPathOld(path, pathCost);

            if (mod_Option.Enabled)
            {
                ((mod_CameraControllerSpline)MonoBehaviourSingleton<Game>.GetInstance(false).cameraController)
                    .IgnoreFollowMob = false;
            }
        }
    }

    [ModifiesType]
    public class mod_InputManager : InputManager
    {
        [ModifiesMember("Update")]
        public void UpdateNew()
        {
            CombatManager instance = MonoBehaviourSingleton<CombatManager>.GetInstance(false);
            if (this.ignoreMousePosition && Input.mousePosition != this.ignoredMousePosition && this.inputMode == InputMode.Keyboard)
            {
                this.ignoreMousePosition = false;
            }
            if (MonoBehaviourSingleton<Game>.GetInstance(false).state == GameState.Gameplay || MonoBehaviourSingleton<Game>.GetInstance(false).state == GameState.RandomEncounter)
            {
                if (this.isGamepadOn)
                {
                    this.CheckForStickMovement();
                }
                this.UpdateCamera();
                if (this.freezeInput)
                {
                    if (!MonoBehaviourSingleton<GUIManager>.GetInstance(false).WillUseMouseClick())
                    {
                        this.UpdateSelection();
                    }
                    this.controllerADown = false;
                    return;
                }
                if (!MonoBehaviourSingleton<GUIManager>.GetInstance(false).WillUseMouseClick())
                {
                    this.UpdateSelection();
                    this.activePCs.Clear();
                    if (instance.inCombat)
                    {
                        Mob currentMob = instance.GetCurrentMob();
                        if (currentMob)
                        {
                            PC asPartyPC = currentMob.asPartyPC;
                            if (asPartyPC)
                            {
                                this.activePCs.Add(asPartyPC);
                            }
                        }
                    }
                    else
                    {
                        foreach (Mob current in this.selectedMobs)
                        {
                            PC asPartyPC2 = current.asPartyPC;
                            if (asPartyPC2 != null)
                            {
                                this.activePCs.Add(asPartyPC2);
                            }
                        }
                        if (this.activePCs.Count == 0)
                        {
                            this.activePCs.Add(MonoBehaviourSingleton<Game>.GetInstance(false).pcLeader);
                        }
                    }
                    if (((cInput.GetButtonDown("Fire1") && !this.isGamepadOn) || (this.controllerADown && this.inAttackMode && this.selectedTargetable)) && Camera.main != null && !MonoBehaviourSingleton<TutorialScreen>.GetInstance(false).popup.gameObject.activeSelf)
                    {
                        if (!this.didDoubleLeftClick && this.ignoreASIDoubleClick)
                        {
                            this.ignoreASIDoubleClick = false;
                        }
                        Targetable targetable = null;
                        Transform transform = null;
                        Transform transform2 = null;
                        Vector3 vector = Vector3.zero;
                        if (this.controllerADown && this.selectedTargetable != null)
                        {
                            targetable = this.selectedTargetable;
                            vector = this.selectedTargetable.transform.position;
                        }
                        else
                        {
                            int layerMask = 1148672;
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit raycastHit;
                            if (Physics.Raycast(ray, out raycastHit, 100f, layerMask) && raycastHit.transform)
                            {
                                if (raycastHit.transform.GetComponent<RaycastPropagate>())
                                {
                                    transform = raycastHit.transform.GetComponent<RaycastPropagate>().target;
                                    targetable = transform.GetComponent<Targetable>();
                                }
                                else
                                {
                                    if (raycastHit.transform.GetComponent<Targetable>())
                                    {
                                        targetable = raycastHit.transform.GetComponent<Targetable>();
                                    }
                                    else
                                    {
                                        if (raycastHit.transform.GetComponent<Drama>())
                                        {
                                            transform2 = raycastHit.transform;
                                        }
                                    }
                                }
                                vector = raycastHit.point;
                            }
                        }
                        if (targetable != null || UseASIManager.GetActiveASIName() == "aoeattack" || UseASIManager.GetActiveASIName() == "coneattack")
                        {
                            bool flag = false;
                            string activeASIName = UseASIManager.GetActiveASIName();
                            if (MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat)
                            {
                                PC pC = MonoBehaviourSingleton<CombatManager>.GetInstance(false).GetCurrentMob() as PC;
                                if (MonoBehaviourSingleton<CombatManager>.GetInstance(false).isPlayersTurn && pC != null)
                                {
                                    flag = (pC.combatActionState == Mob.CombatActionState.THINKING);
                                }
                            }
                            else
                            {
                                if (activeASIName == "attack" || activeASIName == "aoeattack" || activeASIName == "coneattack" || activeASIName == "specialAttack" || activeASIName == "specialConeAttack")
                                {
                                    flag = true;
                                }
                                else
                                {
                                    if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.Hostile || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.ConeAttack || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialAttack || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialConeAttack)
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        if (this.isGamepadOn && this.inAttackMode)
                                        {
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            bool flag2 = MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.Reload || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.CantReload || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.Jammed;
                            if (flag && MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialAttack)
                            {
                                ItemTemplate_Weapon weaponTemplate = this.GetFirstSelectedPlayer().pcStats.GetWeaponTemplate(false);
                                bool flag3 = weaponTemplate is ItemTemplate_WeaponAoe || weaponTemplate is ItemTemplate_WeaponShotgun || weaponTemplate.inventoryWeaponType == InventoryWeaponType.SMG || weaponTemplate.inventoryWeaponType == InventoryWeaponType.HeavyGun;
                                if (targetable is NPC && !flag3)
                                {
                                    MonoBehaviourSingleton<GUIManager>.GetInstance(false).OpenPrecisionAttackMenu(targetable as NPC);
                                }
                                else
                                {
                                    MonoBehaviourSingleton<GlobalFxHandler>.GetInstance(false).GenericUIFail();
                                }
                            }
                            else
                            {
                                if ((!MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat || flag) && (activeASIName == "attack" || activeASIName == "aoeattack" || activeASIName == "coneattack" || activeASIName == "specialAttack" || activeASIName == "specialConeAttack" || flag2 || (this.inAttackMode && this.attackTypeSelected)))
                                {
                                    bool flag4 = false;
                                    this.attackTypeSelected = false;
                                    if (activeASIName == "aoeattack")
                                    {
                                        flag4 = true;
                                    }
                                    foreach (PC current2 in this.GetMovementPCs(null))
                                    {
                                        if (current2.IsJammed())
                                        {
                                            if (current2.CanUnjam())
                                            {
                                                EventInfo_CommandUnjam eventInfo_CommandUnjam = ObjectPool.Get<EventInfo_CommandUnjam>();
                                                eventInfo_CommandUnjam.target = current2;
                                                MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandUnjam, false);
                                            }
                                        }
                                        else
                                        {
                                            if (current2.IsOutOfAmmo())
                                            {
                                                if (current2.CanReload())
                                                {
                                                    EventInfo_CommandReload eventInfo_CommandReload = ObjectPool.Get<EventInfo_CommandReload>();
                                                    eventInfo_CommandReload.mob = current2;
                                                    MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandReload, false);
                                                }
                                                else
                                                {
                                                    HUD_UpdateCharacterCombatInfo.PlayReloadFailedSFX(current2.CheckReload(true, false));
                                                }
                                            }
                                            else
                                            {
                                                if (current2 != targetable)
                                                {
                                                    bool meleeMoveToRange = false;
                                                    if (this.isGamepadOn)
                                                    {
                                                        meleeMoveToRange = true;
                                                    }
                                                    else
                                                    {
                                                        if (this.SelectedLeaderPC() == current2)
                                                        {
                                                            meleeMoveToRange = true;
                                                        }
                                                    }
                                                    if (flag4)
                                                    {
                                                        if (this.SelectedLeaderPC() == current2)
                                                        {
                                                            current2.UseAOEWeapon(MonoBehaviourSingleton<CursorManager>.GetInstance(false).arcAimPosition, MonoBehaviourSingleton<CursorManager>.GetInstance(false).spherePosition);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!(current2.stats.GetWeaponTemplate(false) is ItemTemplate_WeaponShotgun) || current2 == this.GetFirstSelectedPlayer())
                                                        {
                                                            if (current2.stats.GetWeaponTemplate(false) is ItemTemplate_WeaponShotgun)
                                                            {
                                                                if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.ConeAttack || MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialConeAttack)
                                                                {
                                                                    EventInfo_CommandAttack eventInfo_CommandAttack = ObjectPool.Get<EventInfo_CommandAttack>();
                                                                    eventInfo_CommandAttack.pc = current2;
                                                                    eventInfo_CommandAttack.coneAttack = true;
                                                                    eventInfo_CommandAttack.coneTargets = MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetTargetsInCone().ToArray();
                                                                    eventInfo_CommandAttack.aimDirection = MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetConeAimPoint();
                                                                    eventInfo_CommandAttack.target = targetable;
                                                                    if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialConeAttack)
                                                                    {
                                                                        eventInfo_CommandAttack.specialAttack = true;
                                                                    }
                                                                    MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandAttack, false);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (targetable != null)
                                                                {
                                                                    EventInfo_CommandAttack eventInfo_CommandAttack2 = ObjectPool.Get<EventInfo_CommandAttack>();
                                                                    eventInfo_CommandAttack2.pc = current2;
                                                                    eventInfo_CommandAttack2.target = targetable;
                                                                    eventInfo_CommandAttack2.meleeMoveToRange = meleeMoveToRange;
                                                                    if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.SpecialAttack)
                                                                    {
                                                                        eventInfo_CommandAttack2.specialAttack = true;
                                                                    }
                                                                    MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandAttack2, false);
                                                                }
                                                                else
                                                                {
                                                                    EventInfo_CommandAttack eventInfo_CommandAttack3 = ObjectPool.Get<EventInfo_CommandAttack>();
                                                                    eventInfo_CommandAttack3.pc = current2;
                                                                    eventInfo_CommandAttack3.target = null;
                                                                    eventInfo_CommandAttack3.intentionalMiss = true;
                                                                    eventInfo_CommandAttack3.aimDirection = vector;
                                                                    eventInfo_CommandAttack3.meleeMoveToRange = false;
                                                                    MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandAttack3, false);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat)
                                                    {
                                                        MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                                                    }
                                                    MonoBehaviourSingleton<Game>.GetInstance(false).cameraController.FollowPC(current2);
                                                    this.actionClick = true;
                                                }
                                            }
                                        }
                                    }
                                    UseASIManager.SetActiveASIName(null);
                                    UseASIManager.SetActiveASIItem(null, null);
                                    this.ignoreASIDoubleClick = true;
                                }
                                else
                                {
                                    if (UseASIManager.GetActiveASIName() == "useItem" && UseASIManager.GetActiveASIItem() != null && UseASIManager.GetActiveASIItem() is ItemInstance_Usable)
                                    {
                                        this.HandleUsableItemClickOnTargetable(targetable);
                                        this.actionClick = true;
                                        this.ignoreASIDoubleClick = true;
                                    }
                                    else
                                    {
                                        if (UseASIManager.GetActiveASIName() == "doctor" && targetable is PC && (targetable as PC).healthState >= PC.HealthState.Unconscious)
                                        {
                                            MonoBehaviourSingleton<InputManager>.GetInstance(false).HandleSkillClick(targetable.transform, false);
                                        }
                                        else
                                        {
                                            if ((!(targetable is PC) || (targetable as PC).mobState == Mob.MobState.DEAD) && (!this.didDoubleLeftClick || !this.ignoreASIDoubleClick))
                                            {
                                                this.CheckInstigateDrama(targetable.transform, vector, this.didDoubleLeftClick);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (transform != null)
                            {
                                if (UseASIManager.GetActiveASIName() == "useItem" && UseASIManager.GetActiveASIItem() != null && UseASIManager.GetActiveASIItem() is ItemInstance_Usable)
                                {
                                    Drama component = transform.GetComponent<Drama>();
                                    InputManager.PrepareUseItemActions(transform.transform, component, targetable, this.didDoubleLeftClick);
                                    MonoBehaviourSingleton<GlobalFxHandler>.GetInstance(false).InventoryItemOnSelectTarget();
                                    this.actionClick = true;
                                    this.ignoreASIDoubleClick = true;
                                }
                                else
                                {
                                    if (UseASIManager.GetActiveASIName() != null)
                                    {
                                        this.CheckInstigateDrama(transform, vector, this.didDoubleLeftClick);
                                        this.actionClick = true;
                                        this.ignoreASIDoubleClick = true;
                                    }
                                    else
                                    {
                                        if (!this.didDoubleLeftClick || !this.ignoreASIDoubleClick)
                                        {
                                            this.CheckInstigateDrama(transform, vector, this.didDoubleLeftClick);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (transform2 != null)
                                {
                                    if (UseASIManager.GetActiveASIName() != null)
                                    {
                                        this.CheckInstigateDrama(transform2, vector, this.didDoubleLeftClick);
                                        this.actionClick = true;
                                        this.ignoreASIDoubleClick = true;
                                    }
                                    else
                                    {
                                        if (!this.didDoubleLeftClick || !this.ignoreASIDoubleClick)
                                        {
                                            this.CheckInstigateDrama(transform2, vector, this.didDoubleLeftClick);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cInput.GetButton("Fire2") && !this.isGamepadOn)
                        {
                            if (cInput.GetButtonDown("Fire2") && MonoBehaviourSingleton<CursorManager>.GetInstance(false).UsingASI && UseASIManager.GetActiveASIName() != "attack")
                            {
                                this.moveClick = false;
                                UseASIManager.SetActiveASIName(null);
                                UseASIManager.SetActiveASIItem(null, null);
                            }
                            else
                            {
                                if (cInput.GetButtonDown("Fire2") && Camera.main)
                                {
                                    this.moveClick = false;
                                    this.rightClickDownTime = Time.time;
                                    if (this.activePCs.Count > 0)
                                    {
                                        int layerMask2 = 100096;
                                        Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
                                        RaycastHit hit = default(RaycastHit);
                                        if (Physics.Raycast(ray2, out hit, 100f, layerMask2) && hit.transform)
                                        {
                                            this.clickPosition = hit.point;
                                            if (hit.collider.transform.gameObject.layer == 8 || hit.collider.transform.gameObject.layer == 15)
                                            {
                                                if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).UsingASI)
                                                {
                                                    this.moveClick = false;
                                                    UseASIManager.SetActiveASIName(null);
                                                    UseASIManager.SetActiveASIItem(null, null);
                                                }
                                                else
                                                {
                                                    this.moveClick = true;
                                                }
                                            }
                                            else
                                            {
                                                if (hit.transform.gameObject.layer == 9)
                                                {
                                                    if (!MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat || MonoBehaviourSingleton<CombatManager>.GetInstance(false).isPlayersTurn)
                                                    {
                                                        Mob component2 = hit.transform.gameObject.GetComponent<Mob>();
                                                        if (component2 is NPC)
                                                        {
                                                            if (!this.IsSelected(component2))
                                                            {
                                                                this.ClearSelection(true);
                                                                this.AddToSelection(component2);
                                                            }
                                                            else
                                                            {
                                                                this.RemoveFromSelection(component2, true);
                                                            }
                                                            bool flag5 = false;
                                                            if (MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat)
                                                            {
                                                                PC firstSelectedPlayer = this.GetFirstSelectedPlayer();
                                                                ItemTemplate_Weapon itemTemplate_Weapon = (!(firstSelectedPlayer != null)) ? null : firstSelectedPlayer.pcStats.GetWeaponTemplate(false);
                                                                bool flag6 = itemTemplate_Weapon == null || itemTemplate_Weapon is ItemTemplate_WeaponAoe || itemTemplate_Weapon is ItemTemplate_WeaponShotgun || itemTemplate_Weapon.inventoryWeaponType == InventoryWeaponType.SMG || itemTemplate_Weapon.inventoryWeaponType == InventoryWeaponType.HeavyGun;
                                                                if (component2.mobState != Mob.MobState.DEAD && firstSelectedPlayer != null && !flag6 && (firstSelectedPlayer.CanAttack(component2, true, false, false, false) || (firstSelectedPlayer.IsMelee() && firstSelectedPlayer.CanMoveAndAttack(component2, false, false) <= firstSelectedPlayer.combatActionPointsRemaining)))
                                                                {
                                                                    this.selectedTargetable = component2;
                                                                    flag5 = true;
                                                                    MonoBehaviourSingleton<GUIManager>.GetInstance(false).OpenPrecisionAttackMenu(null);
                                                                }
                                                                else
                                                                {
                                                                    MonoBehaviourSingleton<GlobalFxHandler>.GetInstance(false).PlayNegatiaveSound(component2.transform);
                                                                }
                                                            }
                                                            if (!flag5)
                                                            {
                                                                this.CheckExamineDrama(hit);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (this.IsSelected(component2) && this.GetNumSelectedPCs() > 1 && MonoBehaviourSingleton<Game>.GetInstance(false).pcLeader != component2)
                                                            {
                                                                this.RemoveFromSelection(component2, true);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (hit.transform.gameObject.layer == 10)
                                                    {
                                                        SkillObject_Examine skillObject_Examine = this.FindAppropriateExamine(hit);
                                                        if (skillObject_Examine == null || !skillObject_Examine.hidden)
                                                        {
                                                            this.CheckExamineDrama(hit);
                                                        }
                                                        else
                                                        {
                                                            this.moveClick = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (this.moveClick && ((!MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && this.thumbstickMoveDirection != Vector3.zero) || (MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && this.controllerADown) || cInput.GetButtonUp("Fire2")) && Camera.main && string.IsNullOrEmpty(UseASIManager.GetActiveASIName()))
                    {
                        this.moveClick = false;
                        if (!this.rightCameraDragging && this.activePCs.Count > 0)
                        {
                            if (MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && this.lastCombatSquare != null && MonoBehaviourSingleton<CombatManager>.GetInstance(false).isPlayersTurn && this.activePCs[0].combatActionState == Mob.CombatActionState.THINKING)
                            {
                                if (MonoBehaviourSingleton<CombatAStar>.GetInstance(false).Search(this.activePCs[0].currentSquare, this.lastCombatSquare, true, true, this.activePCs[0].combatActionPointsRemaining - this.activePCs[0].GetActionPointsToStand(), this.activePCs[0].stats.GetCombatSpeed()).Count > 0)
                                {
                                    int pathCost = MonoBehaviourSingleton<CombatAStar>.GetInstance(false).GetPathCost(this.activePCs[0].stats.GetCombatSpeed(), this.activePCs[0].GetActionPointsToStand());
                                    if (pathCost > 0)
                                    {
                                        MonoBehaviourSingleton<GlobalFxHandler>.GetInstance(false).Combat_OnMove();
                                        EventInfo_CommandMove eventInfo_CommandMove = ObjectPool.Get<EventInfo_CommandMove>();
                                        eventInfo_CommandMove.mob = this.activePCs[0];
                                        eventInfo_CommandMove.path = MonoBehaviourSingleton<CombatAStar>.GetInstance(false).VectorSmoothPath();
                                        eventInfo_CommandMove.actionPointCost = pathCost;
                                        MonoBehaviourSingleton<EventManager>.GetInstance(false).Publish(eventInfo_CommandMove, false);
                                        MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                                        MonoBehaviourSingleton<CursorManager>.GetInstance(false).ClearCoverCursor();
                                        if (!mod_Option.Enabled) // MODIFIED - ADDED THIS IF STATEMENT TO PREVENT NEXT LINE
                                            MonoBehaviourSingleton<Game>.GetInstance(false).cameraController.FollowPC(this.activePCs[0]);
                                        this.lastCombatSquare = null;
                                        this.timerCombatSquare = null;
                                    }
                                    else
                                    {
                                        MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                                    }
                                }
                                else
                                {
                                    MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                                }
                                MonoBehaviourSingleton<CursorManager>.GetInstance(false).RemoveText(this.cursorTextId);
                                this.cursorTextId = -1;
                            }
                            else
                            {
                                if (!MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat)
                                {
                                    bool flag7 = !NavMesh.SamplePosition(this.clickPosition, out this.navHit, 10f, 1 << InputManager.navMeshLayerIndex_Default);
                                    if (!flag7 && Vector2.Distance(new Vector2(this.navHit.position.x, this.navHit.position.z), new Vector2(this.clickPosition.x, this.clickPosition.z)) > 0.8f)
                                    {
                                        flag7 = true;
                                    }
                                    if (flag7)
                                    {
                                        this.blockedCursor.ShowAt(this.clickPosition + Vector3.up * 0.05f);
                                    }
                                    else
                                    {
                                        bool sprint;
                                        if (this.thumbstickMoveDirection == Vector3.zero)
                                        {
                                            this.moveCursor.ShowAt(this.clickPosition + Vector3.up * 0.05f);
                                            sprint = this.didDoubleRightClick;
                                            this.wasSprintLastFrame = false;
                                        }
                                        else
                                        {
                                            sprint = this.wasSprintLastFrame;
                                            if (this.thumbstickMoveDirection.magnitude > 0.6f)
                                            {
                                                sprint = true;
                                            }
                                            else
                                            {
                                                if (this.thumbstickMoveDirection.magnitude < 0.4f)
                                                {
                                                    sprint = false;
                                                }
                                            }
                                        }
                                        this.wasSprintLastFrame = sprint;
                                        this.MoveInFormation(this.formation, this.clickPosition, sprint, null, 0f, null, true);
                                    }
                                }
                            }
                        }
                        this.thumbstickMoveDirection = Vector3.zero;
                    }
                    if (cInput.GetButtonUp("Fire2"))
                    {
                        this.rightClickUpTime = Time.time;
                    }
                    if (cInput.GetButtonUp("Fire1"))
                    {
                        this.leftClickUpTime = Time.time;
                    }
                    if (!this.ignoreMousePosition && MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && MonoBehaviourSingleton<CombatManager>.GetInstance(false).isPlayersTurn && this.activePCs.Count > 0 && this.activePCs[0].combatActionState == Mob.CombatActionState.THINKING && string.IsNullOrEmpty(UseASIManager.GetActiveASIName()))
                    {
                        Ray ray3 = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit raycastHit2 = default(RaycastHit);
                        this.drawCombatPath = false;
                        if (Physics.Raycast(ray3, out raycastHit2, 1000f, this.layerMaskCombatPath) && raycastHit2.transform)
                        {
                            if (raycastHit2.transform.gameObject.layer == 8 || raycastHit2.transform.gameObject.layer == 15)
                            {
                                this.mouseoverSquare = MonoBehaviourSingleton<CombatAStar>.GetInstance(false).GetNode(raycastHit2.point, raycastHit2.transform.gameObject);
                                bool flag8 = MonoBehaviourSingleton<CombatAStar>.GetInstance(false).IsNodeOpen(this.mouseoverSquare);
                                if (this.mouseoverSquare != null && flag8)
                                {
                                    this.drawCombatPath = true;
                                    if (this.timerCombatSquare != this.mouseoverSquare)
                                    {
                                        this.timerCombatSquare = this.mouseoverSquare;
                                        this.combatSquareTimer = Time.time;
                                    }
                                    if (Time.time - this.combatSquareTimer > 0.1f && this.lastCombatSquare != this.mouseoverSquare)
                                    {
                                        this.lastCombatSquare = this.mouseoverSquare;
                                        if (MonoBehaviourSingleton<CombatAStar>.GetInstance(false).Search(this.GetFirstSelectedPlayer().currentSquare, this.mouseoverSquare, true, true, this.GetFirstSelectedPlayer().combatActionPointsRemaining - this.GetFirstSelectedPlayer().GetActionPointsToStand(), this.GetFirstSelectedPlayer().stats.GetCombatSpeed()).Count > 0)
                                        {
                                            int pathCost2 = MonoBehaviourSingleton<CombatAStar>.GetInstance(false).GetPathCost(this.GetFirstSelectedPlayer().stats.GetCombatSpeed(), this.GetFirstSelectedPlayer().GetActionPointsToStand());
                                            bool attackApRemaining = true;
                                            if (this.GetFirstSelectedPlayer().combatActionPointsRemaining - pathCost2 < this.GetFirstSelectedPlayer().GetActionPointsToAttack())
                                            {
                                                attackApRemaining = false;
                                            }
                                            MonoBehaviourSingleton<CombatAStar>.GetInstance(false).DrawSmoothPath(attackApRemaining, null);
                                            if (pathCost2 != -1 && pathCost2 <= this.GetFirstSelectedPlayer().combatActionPointsRemaining)
                                            {
                                                MonoBehaviourSingleton<CursorManager>.GetInstance(false).ClearCursor(null);
                                                MonoBehaviourSingleton<CursorManager>.GetInstance(false).RemoveText(this.cursorTextId);
                                                this.cursorTextId = MonoBehaviourSingleton<CursorManager>.GetInstance(false).AddText(this.cursorTextId, pathCost2 + " " + Language.Localize("<@>AP", false, false, string.Empty), MonoBehaviourSingleton<GUIManager>.GetInstance(false).largeText, Color.green, false, false, default(Vector3));
                                            }
                                        }
                                        else
                                        {
                                            MonoBehaviourSingleton<CombatAStar>.GetInstance(false).DrawInvalidPath(this.mouseoverSquare);
                                            MonoBehaviourSingleton<CursorManager>.GetInstance(false).RemoveText(this.cursorTextId);
                                            this.cursorTextId = -1;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (this.lastCombatSquare != null)
                                {
                                    this.lastCombatSquare = null;
                                    MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                                    MonoBehaviourSingleton<CursorManager>.GetInstance(false).RemoveText(this.cursorTextId);
                                    this.cursorTextId = -1;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && !this.moveClick && Camera.main && !this.isGamepadOn)
                        {
                            bool flag9 = true;
                            Ray ray4 = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit raycastHit3 = default(RaycastHit);
                            if (Physics.Raycast(ray4, out raycastHit3, 1000f, 33024) && raycastHit3.transform)
                            {
                                NavMeshHit navMeshHit;
                                NavMesh.SamplePosition(raycastHit3.point, out navMeshHit, 1f, 1);
                                if (Vector3.Distance(raycastHit3.point, navMeshHit.position) > 1f)
                                {
                                    flag9 = false;
                                }
                            }
                            else
                            {
                                flag9 = false;
                            }
                            if (!flag9 && MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.Default)
                            {
                                MonoBehaviourSingleton<CursorManager>.GetInstance(false).SetCursor(CursorManager.Cursors.InvalidTerrain, false);
                            }
                            else
                            {
                                if (flag9 && MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.InvalidTerrain)
                                {
                                    MonoBehaviourSingleton<CursorManager>.GetInstance(false).SetCursor(CursorManager.Cursors.Default, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MonoBehaviourSingleton<CursorManager>.GetInstance(false).GetCurrentCursor() == CursorManager.Cursors.InvalidTerrain)
                    {
                        MonoBehaviourSingleton<CursorManager>.GetInstance(false).SetCursor(CursorManager.Cursors.Default, false);
                    }
                }
                if (!this.drawCombatPath && this.lastCombatSquare != null)
                {
                    this.lastCombatSquare = null;
                    if (this.activePCs.Count > 0)
                    {
                        MonoBehaviourSingleton<CombatAStar>.GetInstance(false).ClearPath();
                    }
                    MonoBehaviourSingleton<CursorManager>.GetInstance(false).RemoveText(this.cursorTextId);
                    this.cursorTextId = -1;
                    if (this.ignoreMousePosition && MonoBehaviourSingleton<CombatManager>.GetInstance(false).inCombat && MonoBehaviourSingleton<CombatManager>.GetInstance(false).isPlayersTurn)
                    {
                        PC pC2 = MonoBehaviourSingleton<CombatManager>.GetInstance(false).GetCurrentMob() as PC;
                        NPC selectedNPC = this.GetSelectedNPC();
                        ItemTemplate_WeaponAoe a = pC2.stats.GetWeaponTemplate(false) as ItemTemplate_WeaponAoe;
                        if (a != null && selectedNPC != null)
                        {
                            MonoBehaviourSingleton<CursorManager>.GetInstance(false).ShowSphere();
                            MonoBehaviourSingleton<CursorManager>.GetInstance(false).SetSpherePosition(selectedNPC.transform.position);
                        }
                    }
                }
            }
            this.controllerADown = false;
        }
    }
}
