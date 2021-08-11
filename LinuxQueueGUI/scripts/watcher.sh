#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
while true; do
	
	#echo $DIR;
	cd $DIR;
	./runner.sh >> ./testCron.log;
	sleep 0.5;
	inotifywait -t 180 -q -e create,move,modify,delete ./queue/ ./running/ ./finished/ >> ./testCron.log && { sleep 5; ./runner.sh >> ./testCron.log; };
	
done

date