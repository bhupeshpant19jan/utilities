#include<stdio.h>
#include<string>

/*Read the complete file in C. Handle all basic error cases!*/
char* read_file(char *file_name){

    FILE *file = NULL;
    char *lineBuffer = (char*)calloc(1,1), line[128];

    file = fopen(file_name, "rb");
    if (NULL == file || !lineBuffer ) {
        printf("Cannot read file [%s]. Application will now exit.", file_name);
        exit(1);
    }

    for( ;fgets(line,sizeof line,file) ; strcat(lineBuffer,line) ) {
        if( strchr(line,'\n'))
            *strchr(line,'\n')=0;

        lineBuffer = (char*)realloc(lineBuffer,strlen(lineBuffer)+strlen(line)+1);
        if( !lineBuffer ) {
            printf("Insufficient memory error");
            exit(1);
        }
    }
    return lineBuffer;
}