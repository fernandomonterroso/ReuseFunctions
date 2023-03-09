int sensorPin = 2;
int ledPins[] = {3, 4, 5, 6, 7, 8, 9, 10};
int ledMarcaje[] = {11,12,13};
int numLeds = sizeof(ledPins) / sizeof(ledPins[0]);
int numLedsMarcaje = sizeof(ledMarcaje) / sizeof(ledMarcaje[0]);
void setup() {
  pinMode(sensorPin, INPUT);
  for (int i = 0; i < numLeds; i++) {
    pinMode(ledPins[i], OUTPUT);
  }
  Serial.begin(9600);
  for (int i = 0; i < numLedsMarcaje; i++) {
    pinMode(ledMarcaje[i], OUTPUT);
  }
}

void loop() {
  
  int sensorValue = digitalRead(sensorPin);
  Serial.print("Valor del sensor: ");
  Serial.println(sensorValue);

  if (sensorValue == 1) {
    for (int i = 0; i < numLeds; i++) {
      digitalWrite(ledPins[i], HIGH);
      delay(10);
    }
    for (int i = numLeds - 1; i >= 0; i--) {
      digitalWrite(ledPins[i], LOW);
      delay(10);
    }
    digitalWrite(11, LOW);
    digitalWrite(13, LOW);
  }else{
    digitalWrite(11, HIGH);
    digitalWrite(12, HIGH);
    digitalWrite(13, HIGH);
  }
}
