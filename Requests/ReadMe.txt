### Author - MJOdorczuk ###

----- ENGLISH -----
Data Parser is a program for simple data operations on .csv, .json and .xml files.
It is predefined for request data of format:

For .csv

Client_Id,Request_id,Name,Quantity,Price
"client's Id","request's Id","name","quantity","price"
...

For .json

{
 "requests":[
 {
 "clientId":"client's Id",
 "requestId":"request's Id",
 "name":"name",
 "quantity":"quantity",
 "price":"price"
 },
 ...
 ]
} 

For .xml

<requests>
 <request>
 <clientId>"client's Id"</clientId>
 <requestId>"request's Id"</requestId>
 <name>"name"</name>
 <quantity>"quantity"</quantity>
 <price>"price"</price>
 </request>
 ...
</requests>

Note that .csv files' labels have to start with capital letter, .json and .xml however start with lower letter.
Moreover in .csv "i" in client_Id is uppercase but lowercase in request_id. .csv ID labes have "_" separator but .json and .xml have none.

File is ignored if it has wrong extention (not .csv, .json or .xml).
Line is ignored if any information is missing, any value is set to 0 or name is set to empty string.

To upload a file just drag it to the window and drop anywhere there. Its name, without extention, should appear on the list in the upper-left corner.
Message will show up if extention of the file is wrong.
To choose file to operate just click its name on the list. After that command list should be enabled.
There are 11 possible commands:
 - Ilość zamówień - displays number of the requests.
 - Ilość zamówień dla klienta o wskazanym identyfikatorze - displays number of the requests for specified client.
 - Łączna kwota zamówień - displays total cost of all requests.
 - Łączna kwota zamówień dla klienta o wskazanym identyfikatorze - displays total cost of all requests of specified client.
 - Lista wszystkich zamówień - displays the list of all requests, it is displayed by default.
 - Lista zamówień dla klienta o wskazanym identyfikatorze - displays the list of specified client's requests.
 - Średnia wartość zamówienia - displays the average cost of request.
 - Średnia wartość zamówienia dla klienta o wskazanym identyfikatorze - displays the average cost of specified client's request.
 - Ilość zamówień pogrupowanych po nazwie - displays number of requests grouped by names.
 - Ilość zamówień pogrupowanych po nazwie dla klienta o wskazanym identyfikatorze - displays number of speciefied client's requests grouped by names.
 - Zamówienia w podanym przedziale cenowym - displays all requests in speciefied cost range.

Client's identifier can be speciefied in the "Wybrany identyfikator" box. It is enabled only if command using client's identifier is on.
Cost range can be speciefied in the "Zakres" box, upper bound is specified by value in the upper field and lower bound in lower field. 
It is enabled only if command using cost range is on.
Data can be sorted by the values in a specified column by clicking on the arrows by the column's label.

----- POLISH -----
Data Parser to program służący do prostych działań na plikach .csv, .json i .xml.
Działa wyłącznie dla konkretnego formatu danych:

Dla .csv

Client_Id,Request_id,Name,Quantity,Price
"identyfikator klienta","identyfikator zamówienia","nazwa","ilość","cena"
...

Dla .json

{
 "requests":[
 {
 "clientId":"identyfikator klienta",
 "requestId":"identyfikator zamówienia",
 "name":"nazwa",
 "quantity":"ilość",
 "price":"price"
 },
 ...
 ]
} 

Dla .xml

<requests>
 <request>
 <clientId>"identyfikator klienta"</clientId>
 <requestId>"identyfikator zamówienia"</requestId>
 <name>"nazwa"</name>
 <quantity>"ilość"</quantity>
 <price>"cena"</price>
 </request>
 ...
</requests>

Uwaga! Etykiety dla plików .csv zaczynają się wielką literą w przeciwieństwie do etykiet plików .json i .xml rozpoczynanych małą literą.
Ponadto dla plików .csv "i" w client_Id pisane jest wielką literą, w request_id zaś już małą.
Do tego etykiety identyfikatorów zawierają w sobie znak "_" w plikach .csv, łącznie zaś pisane są w .json i .xml.

Plik jest ignorowany jeżeli jego rozszerzenie nie jest jednym z .csv, .json lub .xml.
Rekord jest ignorowany, gdy brak którejkolwiek informacji, którakolwiek wartość ustawiona jest na 0 lub nazwa jest pustym ciągiem.

By wprowadzić do programu plik przeciągnij go nad okno programu i upuść w dowolnym miejscu na nim. 
Nazwa pliku, bez rozszerzenia powinna pojawić się na liście w lewym-górnym rogu okna.
Jeżeli rozszerzenie jest błędne zostanie wyświetlone okno z odpowiednią informacją.
By wybrać plik kliknij na jego nazwę na liście. Uaktywni to listę komend znajdującą się obok.
Program obsługuje 11 następujących komend:
 - Ilość zamówień
 - Ilość zamówień dla klienta o wskazanym identyfikatorze
 - Łączna kwota zamówień
 - Łączna kwota zamówień dla klienta o wskazanym identyfikatorze
 - Lista wszystkich zamówień
 - Lista zamówień dla klienta o wskazanym identyfikatorze
 - Średnia wartość zamówienia
 - Średnia wartość zamówienia dla klienta o wskazanym identyfikatorze
 - Ilość zamówień pogrupowanych po nazwie
 - Ilość zamówień pogrupowanych po nazwie dla klienta o wskazanym identyfikatorze
 - Zamówienia w podanym przedziale cenowym

Identyfikator klienta może zostać wybrany w polu "Wybrany identyfikator" po prawej stronie okna.
Możliwość ta istnieje wyłącznie gdy została wybrana komenda oczekująca takiej informacji.
Przedział cenowy może zostać wybrany w polu "Zakres", odpowiednio górne pole odpowiada za górne ograniczenie, dolne zaś za dolne.
Możliwość ta istnieje wyłącznie gdy została wybrana komenda oczekująca takiej informacji.
Dane mogą zostać posortowane po wartościach ze wskazanej kolumny poprzez kliknięcie odpowiednije strzałki obok etykiety tej kolumny.