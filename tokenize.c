std::vector<std::string> tokenize(std::string data_string, std::string delimiter = " "){
    /*Tokenize the string and return the vector<std::string>*/
    std::vector<std::string> tokens;
    data_string = data_string.substr(1, data_string.length() - 2);
    int next = -1, pre = 0;
    while(1){
        next = data_string.find(delimiter, pre);
        if (next == std::string::npos){
            if(pre > 0){
                tokens.push_back(data_string.substr(pre));
            }
            break;
        }
        tokens.push_back(data_string.substr(pre, next - pre));
        pre = next +1;
    }
    return tokens;
}

