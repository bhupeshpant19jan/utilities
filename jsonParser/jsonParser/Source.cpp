#include<stdio.h>
#include "json.h"

char input_json_string[] = "{\"success\": true, \"message\": [{\"reportType\": \"tataa\", \"reportId\": \"14 - 32181\", \"title\": \"when you want something\", \"ThreatScape\": [\"1234ddfdc\"], \"publishDate\": 1407936798, \"reportLink\": \"czc\", \"webLink\": \"sdsd\"},{\"reportType\": \"42343243\", \"reportId\": \"14 - 00000197\", \"title\": \"AutomationCreateThreat - 1412085492\",  \"ThreatScape\": [\"Hacktivism\"], \"publishDate\": 1412071320, \"reportLink\": \"zxzxzxzxz\", \"webLink\": \"nmnmnmmm\"}]}";
/*

{
"success": true,
"message": [{
"reportType": "tutifuti",
"reportId": "14-32181",
"title": "when you want something",
"ThreatScape": ["abcxyz"],
"publishDate": 1407936798,
"reportLink": "czc",
"webLink": "sdsd"
},
{
"reportType": "42343243",
"reportId": "14-00000197",
"title": "when you want something - part 2",
"ThreatScape": ["abcxyz"],
"publishDate": 1412071320,
"reportLink": "zxzxzxzxz",
"webLink": "nmnmnmmm"
}]
}


*/
void extract_data_from_json(char* input_json){

    cJSON* root = NULL;
    cJSON* message = NULL;
    cJSON* report_data = NULL;
    cJSON* report_title = NULL;
    cJSON* report_id = NULL;
    int i = -1;
    int total_reports = 0;
    
    if (NULL == input_json_string)
        return;

    root = cJSON_Parse(input_json_string);
    if (NULL == root){
        return;
    }

    message = cJSON_GetObjectItem(root, "message");
    if (NULL == message){
        return;
    }
    total_reports = cJSON_GetArraySize(message); // shows the no of elements!
    for (i = 0; i < total_reports; ++i){
        report_data = cJSON_GetArrayItem(message, i);
        report_id = cJSON_GetObjectItem(report_data, "reportId");
        report_title = cJSON_GetObjectItem(report_data, "title");
        printf("%s", report_data->valuestring);
    }
}
void main(){

    extract_data_from_json(input_json_string);
}