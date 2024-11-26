# Oncall Prober
Добавил Oncall Prober для сервиса Oncall

Рассматриваются след. метрики:
* `prober_create_team_scenario_total` – кол-во проверочных запросов на создание команды в Oncall
* `prober_create_team_scenario_success_total` – кол-во успешных проверочных запросов на создание команды в Oncall
* `prober_create_team_scenario_fail_total` – кол-во неудачных проверочных запросов на создание команды в Oncall
* `prober_create_team_scenario_duration_seconds` – время потраченное на каждый проверочный запрос на создание команды в Oncall

Примеры манифестов для запуска находятся в папке [manifests](./manifests)

# SLA Script
Сервис, который собирает текущие показатели (только созданные в Oncall Prober) с Prometheus (SLI), имеет в себе цели для метрик (SLO) и сохраняет эти показатели в БД
