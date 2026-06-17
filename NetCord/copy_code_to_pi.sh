rsync -rav -e ssh --exclude="bin/*" --exclude="obj/*" \
    ./ \
    cinneyyy@192.168.5.82:/home/cinneyyy/Desktop/Bot/
