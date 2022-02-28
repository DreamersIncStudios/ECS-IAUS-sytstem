using System.Timers;
using TMPro;
using UnityEngine;
namespace GameModes.DestroyTheTower.TimerSystems
{
    [System.Serializable]
    public class TimerCountDown {
        int DurationMinutes;
        int DurationSeconds;
        Timer timer1;
        public TextMeshProUGUI Display;
        public string DisplayTimeLeft { get { return
                    string.Format("{0:00}:{1:00}", DurationMinutes,DurationSeconds );
                   } }
        public TimerCountDown(int TimeOnClock, TextMeshProUGUI displayTimes) {
            timer1 = new Timer(1000);
            timer1.Stop();
            timer1.AutoReset = true;
            DurationMinutes = TimeOnClock;
            timer1.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            this.Display = displayTimes;
            Display.text = DisplayTimeLeft; //Todo make this a UI Element;

        }
        public void StartCountdown() {
            timer1.Start();
        }
        public void Stop() {
            timer1.Stop();
        }
        private  void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (DurationSeconds == 0 && DurationMinutes == 0)
            {
                timer1.Stop();
            }
            else if (DurationSeconds > 0)
            {
                DurationSeconds--;
            }
            else if (DurationSeconds == 0 && DurationMinutes > 0) {
                DurationMinutes--;
                DurationSeconds = 59;
            }
     
            //Todo make this a UI Element;
        }
    
    }
}