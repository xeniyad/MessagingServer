# MessagingServer
Задание

Отказоустойчивый сервер хранения файлов и клиент для закачки файлов.

Необходимо разработать службу сервера, которая будет делать следующее:

· При установке (или первом запуске) создавать очередь для приема файлов

· Слушает эту входящую очередь и сохраняет на диск все пришедшие файлы, группируя в папки по идентификатору (любой) клиента.

Необходимо разработать клиент (Console или Windows на выбор), который будет отправлять выбранные пользователем файлы на сервер через очередь сообщений.

Для упрощения архитектуры примем, что в сети одновременно могут работать несколько клиентов но только 1 центральный сервис.

Примечание!!! Одна из сложностей данного задания: лимит на размер одного сообщения. Суть в том, что как правило, очереди сообщений лимитируют размер одного сообщения (особенно это касается облачных платформ) и получаемый файл, скорее всего, этим лимитам удовлетворять не будет.

Для пересылки больших объемов данных в очереди (там, где имеется лимит и невозможно использовать другие способы передачи) используется подход Message Sequence (по ссылке приведен только рисунок, но общую идею он проясняет).

Обсудите с ментором, какой вариант борьбы с ограничением на размер сообщения вы выберите.

Задание 2. Механизм централизованного контроля и управления

Разработайте механизм централизованного управления через очереди сообщений.

В рамках этого механизма:

· От служб ввода на центральный сервер с некоторой периодичностью приходит текущий статус службы (ждет новых файлов/закачивает файл)

· От сервера ко всем клиентам периодически уходит информация о статусе сервера (работает/не работает)