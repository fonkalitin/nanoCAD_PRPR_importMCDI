Модуль для Нанокад СПДС (nanoCAD SPDS). 
Данный модкль работает только в нанокад СПДС начиная с версии 24.0 т.к. только начиная с этой версии в API был добавлен метод .import("MCDI")
Модуль выполняет:
- проверку подключенной в данный момент БД (сетевая/локальная);
- автоматический импорт файлов MCDI.
Все настройки хранятся во внешнем конфиг-файле "prPr_DBConfig.ini" который должен быть расположен в одном каталоге вместе с *.dll.
