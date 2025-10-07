using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// It was becoming tedious to keep recreating variables for character names everwhere,
// this namespace solves this issue. If a new character is ever added to the game it should also be added here.
namespace CharacterData
{
    public enum Character
    {
        HaroldAuden,
        IainSchweblin,
        MackenzieJoyce,
        EvelynVentress
    }

    public static class CharacterNames
    {
        public static string GetFirstName(Character character)
        {
            switch (character)
            {
                case Character.HaroldAuden: return "Harold";
                case Character.IainSchweblin: return "Iain";
                case Character.MackenzieJoyce: return "Mackenzie";
                case Character.EvelynVentress: return "Evelyn";
                default: return "";
            }
        }

        public static string GetLastName(Character character)
        {
            switch (character)
            {
                case Character.HaroldAuden: return "Auden";
                case Character.IainSchweblin: return "Schweblin";
                case Character.MackenzieJoyce: return "Joyce";
                case Character.EvelynVentress: return "Ventress";
                default: return "";
            }
        }

        public static string GetFullNameSpaced(Character character)
        {
            return $"{GetFirstName(character)} {GetLastName(character)}";
        }

        public static string GetFullNameUnspaced(Character character)
        {
            return $"{GetFirstName(character)}{GetLastName(character)}";
        }

        public static Sprite GetPortrait(Character character)
        {
            string path = $"Portraits/{GetFullNameUnspaced(character)}";
            Sprite portrait = Resources.Load<Sprite>(path);
            
            if (portrait == null)
            {
                Debug.LogError($"Portrait not found at Resources/{path}");
            }
            
            return portrait;
        }

        public static Dictionary<Character, Sprite> GetAllPortraits()
        {
            Dictionary<Character, Sprite> portraits = new Dictionary<Character, Sprite>();
            
            for (int i = 0; i < GetNumCharacters(); i++)
            {
                Character character = (Character)i;
                portraits[character] = GetPortrait(character);
            }
            
            return portraits;
        }

        public static int GetNumCharacters()
        {
            return System.Enum.GetValues(typeof(Character)).Length;
        }
    }
}