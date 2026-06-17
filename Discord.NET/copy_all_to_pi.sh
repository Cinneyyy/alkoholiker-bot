rsync -rav -e ssh --exclude="bin/*" --exclude="obj/*" --exclude="datapath.txt" \
    ./ \
    cinneyyy@192.168.5.82:/home/cinneyyy/Desktop/AlkoholikerBot/