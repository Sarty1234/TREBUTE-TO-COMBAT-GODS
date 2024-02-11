using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Player;
using Unity.Netcode.Components;

namespace Player
{
    //////////////////////////////////////////оголошуємо класи необхідні для роботи модуля
    //
    //
    //
    //
    //Клас множника вхідної шкоди
    public class damageMuliplayerClass
    {
        public float multiplayer;
        public float time;
    }


    //Клас збільшення максимального хп
    public class maxHPAdditionClass
    {
        public float hpAddition;
        public float time;
    }



    public abstract class IHPScript : NetworkBehaviour
    {
        [SerializeField] public TextMesh hptext;

        public float ActualMaxHP;
        public NetworkVariable<float> CurentMaxHP = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> CurentHP = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public maxHPAdditionClass[] maxHPAdditions;


        public damageMuliplayerClass[] damageMultipliers;


        public void setHPText()
        {
            if (hptext == null) return;


            hptext.text = $"{CurentHP.Value}/{CurentMaxHP.Value}";
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetHP();
        }


        public void ResetHP()
        {
            if (!IsOwner) return;


            //Присвоює дані про хп
            CurentHP.Value = ActualMaxHP;
            CurentMaxHP.Value = ActualMaxHP;


            damageMultipliers = new damageMuliplayerClass[0];
            maxHPAdditions = new maxHPAdditionClass[0];
        }


        [ClientRpc]
        public void ResetHPClientRPC(ulong playerID, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log($"Trying to reset player hp {playerID}; curent id - {GetComponent<NetworkTransform>().OwnerClientId}; Is owner - {IsOwner}");
            if (!IsOwner) return;
            


            Debug.Log($"Reseting player hp {playerID}");


            //Присвоює дані про хп
            CurentHP.Value = ActualMaxHP;
            CurentMaxHP.Value = ActualMaxHP;


            damageMultipliers = new damageMuliplayerClass[0];
            maxHPAdditions = new maxHPAdditionClass[0];
        }


        private void Update()
        {
            CalculateMaxHP();
            setHPText();
            CheckIfDead();
        }


        /// <summary>
        /// Публічний метод для нанесення шкоди
        /// </summary>
        /// <param name="damage">Шкода, яку потрібно нанести</param>
        /// <remarks></remarks>
        public void TakeDamage(float damage)
        {
            if (!IsOwner) return;


            //вилучаємо множники шкоди чий час пройшов
            if (damageMultipliers.Length > 0)
            {
                damageMultipliers = damageMultipliers.Where(obj => obj.time >= Time.time).ToArray();
            }


            //обраховуємо нанесену шкоду
            damageMuliplayerClass defoultDamageMuliplayer = new damageMuliplayerClass()
            {
                multiplayer = 1,
                time = -1
            };
            float _damageMultiplier = damageMultipliers.DefaultIfEmpty(defoultDamageMuliplayer).First().multiplayer;
            float _damage = damage * _damageMultiplier;


            //наносимо шкоду
            CurentHP.Value -= _damage;


            //дивимося чи живий обєкт
            CheckIfDead();
        }


        public void CheckIfDead()
        {
            //дивимося чи живий обєкт
            if (CurentHP.Value <= 0)
            {
                //якщо помер то викликаємо івент про його смерть
                Dead();
            }
        }


        /// <summary>
        /// Публічний метод про смерть
        /// </summary>
        /// <remarks></remarks>
        public virtual void Dead()
        {

        }




        /// <summary>
        /// Публічний метод для отримання бустерів вхідної шкоди
        /// </summary>
        /// <param name="damgeMultiplyer">Бустер вхідної шкоди</param>
        /// <param name="duration">Час дії бустера вхідної шкоди</param>
        /// <remarks></remarks>
        public void RecieveDamageMultiplyer(float damgeMultiplyer, float duration)
        {
            if (!IsOwner) return;


            //створюємо клас бустеру вхідної шкоди
            damageMuliplayerClass _damageMuliplayer = new damageMuliplayerClass()
            {
                multiplayer = damgeMultiplyer,
                time = duration + Time.time
            };


            //добавляємо клас бустеру вхідної шкоди до реєстру нашого об'єкту
            damageMultipliers = damageMultipliers.Append(_damageMuliplayer).ToArray();
        }




        /// <summary>
        /// Публічний метод для отримання бустерів максимального хп
        /// </summary>
        /// <param name="HPAddition">Бустер максимального хп</param>
        /// <param name="duration">Час дії бустера вхідної шкоди</param>
        /// <remarks></remarks>
        public void AddMaxHPMultiplyer(float HPAddition, float duration)
        {
            if (!IsOwner) return;


            //створюємо клас бустеру максимального хп
            maxHPAdditionClass maxHPAddition = new maxHPAdditionClass()
            {
                hpAddition = HPAddition,
                time = duration + Time.time
            };


            //добавляємо клас бустеру вхідної шкоди до реєстру нашого об'єкту
            maxHPAdditions = maxHPAdditions.Append(maxHPAddition).ToArray();


            //хілимо об'єкт на кількість доданого хп
            Heal(HPAddition);
        }




        /// <summary>
        /// Приватний метод який обраховує максимальне хп
        /// </summary>
        /// <remarks></remarks>
        private void CalculateMaxHP()
        {
            if (!IsOwner) return;
            float maxHPAddition = 0;


            //очищаємо список від застарілих елементів
            if(maxHPAdditions != null)
            {
                if (maxHPAdditions.Length > 0)
                {
                    maxHPAdditions = maxHPAdditions.Where(obj => obj.time >= Time.time).ToArray();
                }
            }



            //розраховуємо нове максимальне хп
            if (maxHPAdditions != null)
            {
                foreach (maxHPAdditionClass item in maxHPAdditions)
                {
                    maxHPAddition += item.hpAddition;
                }
            }
            


            //присвоюємо нове максимальне хп
            CurentMaxHP.Value = ActualMaxHP + maxHPAddition;
        }



        public void Heal(float heal)
        {
            if (!IsOwner) return;


            //хілимо
            CurentHP.Value += heal;


            //перевірямо чи не перевищує хп максимальне
            if (CurentHP.Value > CurentMaxHP.Value) CurentHP.Value = CurentMaxHP.Value;
        }
    }
}
