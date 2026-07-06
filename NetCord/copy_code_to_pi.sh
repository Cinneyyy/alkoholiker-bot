rsync -rav -e ssh --exclude="bin/*" --exclude="obj/*" \
    ./ \
    cinneyyy@rb.pi:/home/cinneyyy/Desktop/Bot/
