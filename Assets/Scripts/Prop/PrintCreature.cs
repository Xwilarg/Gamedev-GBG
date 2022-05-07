﻿using GamedevGBG.BodyPart;
using System.Linq;
using UnityEngine;

namespace GamedevGBG.Prop
{
    public class PrintCreature : MonoBehaviour
    {
        [SerializeField]
        private Transform _spawnPoint;

        public void Print(string key)
        {
            var elems = key.Split(';').Select(x => VialManager.Instance.GetByKey(x));

            var torsos = elems.Where(x => x.Type == ContentType.Body).ToArray();
            var heads = elems.Where(x => x.Type == ContentType.Head).ToArray();
            var tails = elems.Where(x => x.Type == ContentType.Tail).ToArray();
            var others = elems.Where(x => x.Type == ContentType.Other).ToArray();
            if (torsos.Any())
            {
                var target = torsos[Random.Range(0, torsos.Length)];
                var targetGo = Instantiate(target.Prefab, _spawnPoint.position, Quaternion.identity);
                var targetGoTorso = targetGo.GetComponent<Torso>();
                if (heads.Any())
                {
                    var targetHead = heads[Random.Range(0, heads.Length)];
                    var targetEnabl = targetHead.Animal switch
                    {
                        AnimalType.Cat => targetGoTorso.CatHead,
                        AnimalType.Alien => targetGoTorso.AlienHead,
                        AnimalType.Dino => targetGoTorso.DinoHead,
                        AnimalType.Cupcake => targetGoTorso.Topping,
                        AnimalType.Tentacles => targetGoTorso.Tentacles,
                        _ => throw new System.NotImplementedException()
                    };
                    if (targetEnabl != null)
                    {
                        targetEnabl.gameObject.SetActive(true);
                        var targetGoHead = targetEnabl.GetComponent<Head>();
                        if (others.Any(x => x.Type == ContentType.Hat && x.Animal == AnimalType.Cupcake))
                        {
                            targetGoHead.Topping.gameObject.SetActive(true);
                        }
                        if (others.Any(x => x.Type == ContentType.Hat && x.Animal == AnimalType.Fish))
                        {
                            targetGoHead.Bowl.gameObject.SetActive(true);
                        }
                    }
                }
                else if (elems.Any(x => x.Animal == AnimalType.Cupcake && x.Type == ContentType.Hat))
                {
                    targetGoTorso.Topping?.gameObject?.SetActive(true);
                }
                if (tails.Any())
                {
                    var targetTail = tails[Random.Range(0, tails.Length)];
                    (targetTail.Animal switch
                    {
                        AnimalType.Cat => targetGoTorso.CatTail,
                        AnimalType.Alien => targetGoTorso.AlienTail,
                        AnimalType.Dino => targetGoTorso.DinoTail,
                        _ => throw new System.NotImplementedException()
                    }).gameObject.SetActive(true);
                }
            }
            else if (heads.Any())
            {
                var targetHead = heads[Random.Range(0, heads.Length)];
                var targetHeadGo = Instantiate(targetHead.Prefab, _spawnPoint.position, Quaternion.identity);
                var targetGoHead = targetHeadGo.GetComponent<Head>();
                if (others.Any(x => x.Type == ContentType.Hat && x.Animal == AnimalType.Cupcake))
                {
                    targetGoHead.Topping.gameObject.SetActive(true);
                }
                if (others.Any(x => x.Type == ContentType.Hat && x.Animal == AnimalType.Fish))
                {
                    targetGoHead.Bowl.gameObject.SetActive(true);
                }
            }
        }
    }
}