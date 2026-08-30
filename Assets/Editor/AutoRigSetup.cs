using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;

public class AutoRigSetup : EditorWindow
{
    [MenuItem("Tools/CorpseParty/Full Auto-Setup Character")]
    public static void SetupCharacter()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Select the SapphiArtchan root object in the Hierarchy first!", "OK");
            return;
        }
        
        GameObject root = selected;
        
        Transform FindBone(string name)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name == name) return t;
            }
            return null;
        }
        
        // ---------- 1. Find bones ----------
        Transform upperArmL = FindBone("UpperArm_L");
        Transform forearmL = FindBone("Forearm_L");
        Transform handL = FindBone("Hand_L");
        Transform upperArmR = FindBone("UpperArm_R");
        Transform forearmR = FindBone("Forearm_R");
        Transform handR = FindBone("Hand_R");
        Transform legL = FindBone("Leg_L");
        Transform kneeL = FindBone("Knee_L");
        Transform footL = FindBone("Foot_L");
        Transform legR = FindBone("Leg_R");
        Transform kneeR = FindBone("Knee_R");
        Transform footR = FindBone("Foot_R");
        
        if (upperArmL == null || forearmL == null || handL == null ||
            upperArmR == null || forearmR == null || handR == null ||
            legL == null || kneeL == null || footL == null ||
            legR == null || kneeR == null || footR == null)
        {
            string missing = "";
            if (upperArmL == null) missing += "UpperArm_L, ";
            if (forearmL == null) missing += "Forearm_L, ";
            if (handL == null) missing += "Hand_L, ";
            if (upperArmR == null) missing += "UpperArm_R, ";
            if (forearmR == null) missing += "Forearm_R, ";
            if (handR == null) missing += "Hand_R, ";
            if (legL == null) missing += "Leg_L, ";
            if (kneeL == null) missing += "Knee_L, ";
            if (footL == null) missing += "Foot_L, ";
            if (legR == null) missing += "Leg_R, ";
            if (kneeR == null) missing += "Knee_R, ";
            if (footR == null) missing += "Foot_R, ";
            
            EditorUtility.DisplayDialog("Error", "Could not find these bones: " + missing, "OK");
            return;
        }
        
        // ---------- 2. Physics setup ----------
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null) rb = root.AddComponent<Rigidbody>();
        rb.mass = 70;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
        if (capsule == null) capsule = root.AddComponent<CapsuleCollider>();
        capsule.radius = 0.3f;
        capsule.height = 2.0f;
        capsule.center = new Vector3(0, 1, 0);
        
        // ---------- 3. Rig setup (skip if already exists) ----------
        Transform existingRig = root.transform.Find("Rig1");
        GameObject rig1;
        Rig rigComponent;
        
        Transform leftArmTarget, leftLegTarget, rightArmTarget, rightLegTarget;
        
        if (existingRig != null)
        {
            rig1 = existingRig.gameObject;
            rigComponent = rig1.GetComponent<Rig>();
            leftArmTarget = rig1.transform.Find("LeftArm_Target");
            leftLegTarget = rig1.transform.Find("LeftLeg_Target");
            rightArmTarget = rig1.transform.Find("RightArm_Target");
            rightLegTarget = rig1.transform.Find("RightLeg_Target");
        }
        else
        {
            rig1 = new GameObject("Rig1");
            rig1.transform.SetParent(root.transform, false);
            rigComponent = rig1.AddComponent<Rig>();
            
            leftArmTarget = CreateIKChain(rig1.transform, "LeftArm", upperArmL, forearmL, handL);
            rightArmTarget = CreateIKChain(rig1.transform, "RightArm", upperArmR, forearmR, handR);
            leftLegTarget = CreateIKChain(rig1.transform, "LeftLeg", legL, kneeL, footL);
            rightLegTarget = CreateIKChain(rig1.transform, "RightLeg", legR, kneeR, footR);
        }
        
        RigBuilder rigBuilder = root.GetComponent<RigBuilder>();
        if (rigBuilder == null) rigBuilder = root.AddComponent<RigBuilder>();
        
        bool alreadyAdded = false;
        foreach (var layer in rigBuilder.layers)
        {
            if (layer.rig == rigComponent) alreadyAdded = true;
        }
        if (!alreadyAdded) rigBuilder.layers.Add(new RigLayer(rigComponent, true));
        
        // ---------- 4. Gameplay scripts ----------
        Animator animator = root.GetComponent<Animator>();
        
        LimbIKController limbController = root.GetComponent<LimbIKController>();
        if (limbController == null) limbController = root.AddComponent<LimbIKController>();
        
        SerializedObject so = new SerializedObject(limbController);
        so.FindProperty("leftArmTarget").objectReferenceValue = leftArmTarget;
        so.FindProperty("leftLegTarget").objectReferenceValue = leftLegTarget;
        so.FindProperty("rightArmTarget").objectReferenceValue = rightArmTarget;
        so.FindProperty("rightLegTarget").objectReferenceValue = rightLegTarget;
        so.ApplyModifiedProperties();
        
        CharacterMovement charMovement = root.GetComponent<CharacterMovement>();
        if (charMovement == null) charMovement = root.AddComponent<CharacterMovement>();
        
        SerializedObject moveSO = new SerializedObject(charMovement);
        moveSO.FindProperty("rb").objectReferenceValue = rb;
        moveSO.FindProperty("animator").objectReferenceValue = animator;
        moveSO.FindProperty("limbController").objectReferenceValue = limbController;
        moveSO.ApplyModifiedProperties();
        
        NetworkPlayer netPlayer = root.GetComponent<NetworkPlayer>();
        if (netPlayer == null) netPlayer = root.AddComponent<NetworkPlayer>();
        
        EditorUtility.SetDirty(root);
        
        EditorUtility.DisplayDialog("Success", 
            "Full character setup complete!\n\n" +
            "- Rigidbody + Collider configured\n" +
            "- IK Rig with 4 targets created\n" +
            "- LimbIKController wired up\n" +
            "- CharacterMovement wired up\n" +
            "- NetworkPlayer added\n\n" +
            "Ready to test!", "OK");
    }
    
    private static Transform CreateIKChain(Transform parent, string limbName, Transform bone1, Transform bone2, Transform bone3)
    {
        GameObject target = new GameObject(limbName + "_Target");
        target.transform.SetParent(parent, false);
        target.transform.position = bone3.position;
        target.transform.rotation = bone3.rotation;
        
        GameObject ikObj = new GameObject(limbName + "_IK");
        ikObj.transform.SetParent(parent, false);
        
        TwoBoneIKConstraint ik = ikObj.AddComponent<TwoBoneIKConstraint>();
        ik.data.root = bone1;
        ik.data.mid = bone2;
        ik.data.tip = bone3;
        ik.data.target = target.transform;
        ik.weight = 1f;
        
        return target.transform;
    }
}