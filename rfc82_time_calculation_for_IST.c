
/**
* @brief get_offset
*
* TODO , TOFIX : [ For Cygwin Compilation] It doesn't give proper time for Timezone value
* Observed for the IST time zone, it appears as +0553
* instead of +0530
*/
static char* get_offset()
{
    int offset = 0;
    struct tm *tptr = NULL;
    int lower = 0;
    int tmp_offset = 0;
    time_t secs, local_secs, gmt_secs;
    char* offset_buff = (char*)calloc(10, sizeof(char));
    long diff_secs = 0;
    int index = 0;

    time(&secs);  /** Current time in GMT*/
    /**Remember that localtime/gmtime overwrite same location*/
    tptr = localtime(&secs);
    local_secs = mktime(tptr);
    tptr = gmtime(&secs);
    gmt_secs = mktime(tptr);
    diff_secs = (long)(local_secs - gmt_secs);
    offset = diff_secs / 36;
    tmp_offset = offset;

    /**This code is to fix the bug on windows API for calculating the time difference.*/
    if (NULL == offset_buff){
        write_log(I_LOG_ERR, MEMORY_ALLOCATION_ERROR);
        exit(1);
    }
    memset(offset_buff, 0, 10);
    offset_buff[9] = '\0';
    lower = tmp_offset % 10;
    tmp_offset = tmp_offset / 10;

    lower = (tmp_offset % 10) * 10 + lower;
    tmp_offset = tmp_offset / 10;

    if (lower == 50)
        lower = 30;
    if (lower == 75 || lower == 70)
        lower = 45;

    lower = tmp_offset * 100 + lower;
    offset = lower;

    //Day light saving calculation
    if (tptr->tm_isdst > 0)
        offset += 100;

    if (offset < 0){
        offset *= -1;
        offset_buff[index++] = '-';
    }
    else
        offset_buff[index++] = '+';
    if (offset < 1000) {
        if (offset == 0){
            snprintf(offset_buff + index, 4, "%s", "000");
            index += 3;
        }
        else
            offset_buff[index++] = '0';
    }
    snprintf(offset_buff + index, 5, "%d", offset);

    return offset_buff;
}

void time_rfc_822_format(char *time_buf)
{
    struct tm *local_time;
    time_t cur_time;
    char* offset = get_offset();

    cur_time = time(NULL);
    local_time = localtime(&cur_time);
#ifdef _LINUX
    strftime(time_buf, 128, "%a, %d %b %Y %H:%M:%S %z", local_time);
#else
    strftime(time_buf, 128, "%a, %d %b %Y %H:%M:%S", local_time);
    sprintf_s(time_buf, 128, "%s %s", time_buf, offset);
#endif
    free(offset);
}