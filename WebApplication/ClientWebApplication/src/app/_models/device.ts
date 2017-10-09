import {Record} from "./Record";
import {Process} from "./Process";
/**
 * Created by DomikoX on 11.3.2017.
 */
export class Device{

    Id: string;
    Name: string;
    CpuInfo: string;
    OsInfo: string;
    Online:boolean;
    CpuUsage: number = 0;
    RamUsage: number[] = [0,0];
    LastUsageTime: Date;
    Records: Record[];
    Processes: Process[];
    IsSelected: boolean;
    LastScreen: string;

    public dosmething(dev){
        console.log(dev);
    }

    public update(other:Device):void{
        this.Name = other.Name;
        this.CpuInfo = other.CpuInfo;
        this.OsInfo = other.OsInfo;
        this.Online = other.Online;
        this.IsSelected = other.IsSelected;
    }

}