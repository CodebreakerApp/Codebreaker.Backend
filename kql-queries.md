# KQL

Resource group: codebreakerapp, Logs, do this query:

AppMetrics |
where AppRoleInstance startswith "game" and Name startswith "codebreaker.game_moves" 
| make-series Games=any(Sum) default=0 on TimeGenerated from ago(4h) to now() step 10min by AppRoleInstance
| render timechart 

Application Insights

Metrics, select the scope of the subscription, App Insights

Select metrics for charts

Logs, select table traces

traces
| where customDimensions.EventName == "Response" // application_Version startswith "botq"

custom metrics only available with app insights