using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

public interface IExtraPropService
{
    ResultWithMessage add(CreateExtraFieldViewModel input);
    ResultWithMessage edit(int id, CreateExtraFieldViewModel input);
    ResultWithMessage delete(int id);
    ResultWithMessage GetAll();


}

public class ExtraPropService : IExtraPropService
{
    private readonly TenorDbContext _db;
    private readonly IMapper _mapper;
    public ExtraPropService(TenorDbContext tenorDbContext, IMapper mapper)
    {
        _db = tenorDbContext;
        _mapper = mapper;

    }


    public ResultWithMessage add(CreateExtraFieldViewModel input)
    {
        _db.ExtraFields.Add(_mapper.Map<ExtraField>(input));
        _db.SaveChanges();
        return new ResultWithMessage(input,null);
    }

    public ResultWithMessage delete(int id)
    {
        ExtraField extField = _db.ExtraFields.FirstOrDefault(x => x.Id == id);
        if (extField is null)
        {
            return new ResultWithMessage(null, "Extra field is invalid");

        }
        _db.ExtraFields.Remove(extField);
        _db.SaveChanges();
        return new ResultWithMessage(true,null);
    }

    public ResultWithMessage edit(int id, CreateExtraFieldViewModel input)
    {
        if(id!=input.Id)
        {
            return new ResultWithMessage(null, "ID Mismatch");
        }
        ExtraField extField = _db.ExtraFields.AsNoTracking().FirstOrDefault(x=>x.Id==id);
        if(extField is null)
        {
            return new ResultWithMessage(null, "Extra field is invalid");

        }

        _db.ExtraFields.Update(_mapper.Map<ExtraField>(input));
        _db.SaveChanges();
        return new ResultWithMessage(input,null);
    }

    public ResultWithMessage GetAll()
    {
        var extFields = _db.ExtraFields.ToList();
        return new ResultWithMessage(_mapper.Map<List<CreateExtraFieldViewModel>>(extFields),null);
    }
}