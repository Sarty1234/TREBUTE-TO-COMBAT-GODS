using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Other
{
    public class PlayerHP : IHPScript
    {
        [SerializeField] public TextMesh hptext;


        [SerializeField] public bool CanRaiseOnRespawning;
        [SerializeField] public bool CanBeRespawned;




        /// <summary>
        /// Публічний метод для івенту про респавн гравця
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        public void Respawn(object sender, PlayerEventsHandler.DyingEventArgs e)
        {
            //перевіряємо чи можна викликати івент OnRespawning
            if (!CanRaiseOnRespawning) return;


            PlayerEventsHandler.RespawningEventArgs args = new PlayerEventsHandler.RespawningEventArgs(this.gameObject);
            PlayerEventsHandler.InvokeOnRespawning(this, args);
        }




        /// <summary>
        /// Публічний метод для самого респавну гравця
        /// </summary>
        /// <remarks></remarks>
        private void RespawnPlayer(object sender, PlayerEventsHandler.RespawningEventArgs e)
        { 
            
        }



        private void Start()
        {
            if (!IsOwner) return;
            //Робимо початкову підготовку змінних
            StartDataAssign();


            //обнуляє дії, які потрібно виконати під час виклику івентів
            //OnRespawning = null;


            //////////////////////////////////Записуємо дії до івентів
            //
            //
            //
            //
            //Записуємо дії до івенту про респавн гравця
            PlayerEventsHandler.Instance.OnRespawning += RespawnPlayer;

            //Записуємо дії до івенту про смерть гравця
            PlayerEventsHandler.Instance.OnDying += Respawn;
            PlayerEventsHandler.Instance.OnUpdate += setHPText;
        }


        public void setHPText()
        {
            if (hptext == null) return;


            hptext.text = $"{CurentHP}/{CurentMaxHP}";
        }
    }
}
