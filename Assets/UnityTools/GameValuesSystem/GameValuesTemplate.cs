

using UnityEngine;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
namespace UnityTools {

    [CreateAssetMenu(menuName="Unity Tools/Game Values Template", fileName="GameValuesTemplate")]
    public class GameValuesTemplate : GameSettingsObject
    {
        [NeatArray] public GameValueArray gameValues;
    }
}

