using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;

public class AutoRigSetup : EditorWindow
{
    [MenuItem("Tools/CorpseParty/Auto-Setup IK Rig")]
    public static void SetupRig()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Select the SapphiArtchan root object in the Hierarchy first!", "OK");
            return;
        }
        
        GameObject root = selected;
        
        // Find bones by name (searches all children recursively)
        Transform FindBone(string name)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name == name) return t;
            }
            return null;
        }
        
        // Try to find arm/leg bones - adjust names here if yours differ
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
        
        // Validate all bones found
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
        
        // Create Rig1 container
        GameObject rig1 = new GameObject("Rig1");
        rig1.transform.SetParent(root.transform, false);
        Rig rigComponent = rig1.AddComponent<Rig>();
        
        // Create targets and IK constraints for each limb
        CreateIKChain(rig1.transform, "LeftArm", upperArmL, forearmL, handL);
        CreateIKChain(rig1.transform, "RightArm", upperArmR, forearmR, handR);
        CreateIKChain(rig1.transform, "LeftLeg", legL, kneeL, footL);
        CreateIKChain(rig1.transform, "RightLeg", legR, kneeR, footR);
        
        // Add RigBuilder to root if missing
        RigBuilder rigBuilder = root.GetComponent<RigBuilder>();
        if (rigBuilder == null)
        {
            rigBuilder = root.AddComponent<RigBuilder>();
        }
        
        // Add Rig1 to RigBuilder's layers
        bool alreadyAdded = false;
        foreach (var layer in rigBuilder.layers)
        {
            if (layer.rig == rigComponent) alreadyAdded = true;
        }
        
        if (!alreadyAdded)
        {
            rigBuilder.layers.Add(new RigLayer(rigComponent, true));
        }
        
        EditorUtility.SetDirty(root);
        EditorUtility.DisplayDialog("Success", "IK Rig setup complete! Check Rig1 in the Hierarchy for your 4 targets.", "OK");
    }
    
    private static void CreateIKChain(Transform parent, string limbName, Transform bone1, Transform bone2, Transform bone3)
    {
        // Create target at the tip bone's world position
        GameObject target = new GameObject(limbName + "_Target");
        target.transform.SetParent(parent, false);
        target.transform.position = bone3.position;
        target.transform.rotation = bone3.rotation;
        
        // Create IK constraint object
        GameObject ikObj = new GameObject(limbName + "_IK");
        ikObj.transform.SetParent(parent, false);
        
        TwoBoneIKConstraint ik = ikObj.AddComponent<TwoBoneIKConstraint>();
        ik.data.root = bone1;
        ik.data.mid = bone2;
        ik.data.tip = bone3;
        ik.data.target = target.transform;
        ik.weight = 1f;
    }
}