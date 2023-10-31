using UnityEngine;

public class ClockScript : MonoBehaviour
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform secondHand;

    private void Update() {
        // Get the current time
        System.DateTime currentTime = System.DateTime.Now;

        // Calculate the rotation angles for each hand
        float secondsAngle = ( currentTime.Second / 60f) * 360f;
        float minutesAngle = ((currentTime.Minute + currentTime.Second / 60f) / 60f) * 360f;
        float hoursAngle = (((currentTime.Hour) % 12 + currentTime.Minute / 60f) / 12f) * 360f;
        //time = new Vector3Int(currentTime.Hour, currentTime.Minute, currentTime.Second);
        
        // Rotate the clock hands
        secondHand.localRotation = Quaternion.Euler(0f,-secondsAngle,  0f);
        minuteHand.localRotation = Quaternion.Euler(0f,-minutesAngle,  0f);
        hourHand.localRotation =   Quaternion.Euler(0f,-hoursAngle  , 0f );
    }
}