using Unity.Netcode;
using UnityEngine;

public class CharacterFunctionnality : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<Character> Hero= new NetworkVariable<Character>(Character.none, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Sprite paladinIconeSprite;
    [SerializeField] private Sprite thiefIconeSprite;
    [SerializeField] private Sprite sorcererIconeSprite;
    [SerializeField] private Sprite bardIconeSprite;
    [SerializeField] private Sprite dwarfIconeSprite;
    [SerializeField] private Sprite mercenaryIconeSprite;

    private bool HasUsedFirstCapacity;
    private bool HasUsedsecondCapacity;

    public void SetPlayerCharacter(Character hero)
    {
        if (!IsOwner)
            return;
        SetPlayerCharacterRpc(hero);

        if (hero == Character.none)
            return;
        else if (hero == Character.paladin)
        {
            HudManager.instance.GetCharacterIcone().sprite = paladinIconeSprite;
        }
        else if (hero == Character.sorcerer)
        {
            HudManager.instance.GetCharacterIcone().sprite = sorcererIconeSprite;
        }
        else if (hero == Character.mercenary)
        {
            HudManager.instance.GetCharacterIcone().sprite = mercenaryIconeSprite;
        }
        else if (hero == Character.bard)
        {
            HudManager.instance.GetCharacterIcone().sprite = bardIconeSprite;
        }
        else if (hero == Character.Dwarf)
        {
            HudManager.instance.GetCharacterIcone().sprite = dwarfIconeSprite;
        }
        else if (hero == Character.thief)
        {
            HudManager.instance.GetCharacterIcone().sprite = thiefIconeSprite;
        }
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerCharacterRpc(Character hero)
    {
        Hero.Value= hero;
    }

    public Character GetPlayerCharacter()
    {
        return Hero.Value;
    }

    public Sprite GetplayerCharacterIcone()
    {
        if (Hero.Value == Character.paladin)
        {
            return paladinIconeSprite;
        }
        else if (Hero.Value == Character.sorcerer)
        {
            return sorcererIconeSprite;
        }
        else if (Hero.Value == Character.mercenary)
        {
            return mercenaryIconeSprite;
        }
        else if (Hero.Value == Character.bard)
        {
            return bardIconeSprite;
        }
        else if (Hero.Value == Character.Dwarf)
        {
            return dwarfIconeSprite;
        }
        else if (Hero.Value == Character.thief)
        {
            return thiefIconeSprite;
        }

        return null;
    }


    public bool IsFirstCapacityUsed()
    { 
        return HasUsedFirstCapacity; 
    }

    public bool IsSecondCapacityUsed()
    {
        return HasUsedsecondCapacity;
    }

    public void UseFirstCapacity()
    {
        if (!IsOwner)
            return;

        if (HasUsedFirstCapacity)
            return;
        HasUsedFirstCapacity = true;

        if (Hero.Value == Character.none)
            return;
        else if (Hero.Value == Character.paladin)
        {

        }
        else if (Hero.Value == Character.sorcerer)
        {

        }
        else if (Hero.Value == Character.mercenary)
        {

        }
        else if (Hero.Value == Character.bard)
        {

        }
        else if (Hero.Value == Character.Dwarf)
        {

        }
        else if (Hero.Value == Character.thief)
        {

        }

    }

    public void UseSecondCapacity()
    {
        if (!IsOwner)
            return;

        if (HasUsedsecondCapacity)
            return;
        HasUsedsecondCapacity = true;

        if (Hero.Value == Character.none)
            return;
        else if (Hero.Value == Character.paladin)
        {

        }
        else if (Hero.Value == Character.sorcerer)
        {

        }
        else if (Hero.Value == Character.mercenary)
        {

        }
        else if (Hero.Value == Character.bard)
        {

        }
        else if (Hero.Value == Character.Dwarf)
        {

        }
        else if (Hero.Value == Character.thief)
        {

        }
    }
}
