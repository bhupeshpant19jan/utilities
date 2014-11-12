
char* get_time(){
	/*return the time in the given format "2014-11-12 17:51:06" */
    /*NOTE:- Free the buffer after use*/
    /*Platform independent code!*/
    char* p = calloc(1024, sizeof(char));
#if defined(_LINUX)
    time_t t;
#endif

#if defined(_WIN32) || defined(_WIN64)
    struct tm *tm;
#else
    struct tm tm;
#endif

#if defined(_WIN32) || defined(_WIN64)
    __time64_t long_time;
    _time64(&long_time);           /* Get time as 64-bit integer. */
    /* Convert to local time. */
    tm = _localtime64(&long_time); /* C4996 */
#endif

#if defined(_LINUX)
    t = time(NULL);
    localtime_r(&t, &tm);
    sprintf(p, "%04d-%02d-%02d %02d:%02d:%02d ", tm.tm_year + 1900, tm.tm_mon + 1, tm.tm_mday, tm.tm_hour, tm.tm_min, tm.tm_sec);
#else
    sprintf(p, "%04d-%02d-%02d %02d:%02d:%02d ", tm->tm_year + 1900, tm->tm_mon + 1, tm->tm_mday, tm->tm_hour, tm->tm_min, tm->tm_sec);
#endif
    return p;
}
