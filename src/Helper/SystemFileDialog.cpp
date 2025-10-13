#include <iostream>
#include <shobjidl_core.h>
#include <string>
#include <vector>

extern "C" {
__declspec(dllexport) std::wstring SetFileTypes(IFileDialog* dialog, const wchar_t* filter)
{
    std::wstring defaultExt;

    if (filter == nullptr || wcslen(filter) == 0)
        return defaultExt;

    std::vector<std::wstring> filterParts;
    std::wstring filterStr(filter);
    size_t pos = 0;

    while ((pos = filterStr.find(L'|')) != std::wstring::npos)
    {
        filterParts.push_back(filterStr.substr(0, pos));
        filterStr.erase(0, pos + 1);
    }
    if (!filterStr.empty())
        filterParts.push_back(filterStr);

    size_t filterCount = filterParts.size() / 2;
    if (filterCount > 0)
    {
        std::vector<COMDLG_FILTERSPEC> fileTypes(filterCount);

        for (size_t i = 0; i < filterCount; i++)
        {
            fileTypes[i].pszName = filterParts[i * 2].c_str();
            fileTypes[i].pszSpec = filterParts[i * 2 + 1].c_str();
        }

        dialog->SetFileTypes(static_cast<UINT>(filterCount), fileTypes.data());
        dialog->SetFileTypeIndex(1);

        if (filterParts.size() > 1)
        {
            std::wstring firstSpec = filterParts[1];
            size_t starPos = firstSpec.find(L"*.");
            if (starPos != std::wstring::npos)
            {
                defaultExt = firstSpec.substr(starPos + 2);
                size_t semiPos = defaultExt.find(L';');
                if (semiPos != std::wstring::npos)
                {
                    defaultExt = defaultExt.substr(0, semiPos);
                }
            }
        }
    }

    return defaultExt;
}

__declspec(dllexport) void create_open_file_dialog(const wchar_t* title, const wchar_t* initialPath, wchar_t* outPath,
                                                   int fosFlags,
                                                   const wchar_t* filter, HRESULT& hr, BOOL& result)
{
    IFileOpenDialog* openDialog = nullptr;
    hr = CoCreateInstance(CLSID_FileOpenDialog, nullptr, CLSCTX_ALL,
                          IID_IFileOpenDialog,
                          reinterpret_cast<void**>(&openDialog));

    if (SUCCEEDED(hr))
    {
        DWORD dwOptions;
        hr = openDialog->GetOptions(&dwOptions);
        if (SUCCEEDED(hr))
        {
            openDialog->SetOptions(dwOptions | fosFlags);
        }

        if (title != nullptr && wcslen(title) > 0)
        {
            openDialog->SetTitle(title);
        }

        if (initialPath != nullptr && wcslen(initialPath) > 0)
        {
            IShellItem* pInitialFolder = nullptr;
            hr = SHCreateItemFromParsingName(initialPath, nullptr, IID_PPV_ARGS(&pInitialFolder));
            if (SUCCEEDED(hr))
            {
                openDialog->SetFolder(pInitialFolder);
                pInitialFolder->Release();
            }
        }

        SetFileTypes(openDialog, filter);

        hr = openDialog->Show(nullptr);

        if (SUCCEEDED(hr))
        {
            if (fosFlags & FOS_ALLOWMULTISELECT)
            {
                IShellItemArray* pItemArray = nullptr;
                hr = openDialog->GetResults(&pItemArray);
                if (SUCCEEDED(hr))
                {
                    DWORD count = 0;
                    pItemArray->GetCount(&count);

                    std::wstring allPaths;
                    for (DWORD i = 0; i < count; i++)
                    {
                        IShellItem* pItem = nullptr;
                        hr = pItemArray->GetItemAt(i, &pItem);
                        if (SUCCEEDED(hr))
                        {
                            PWSTR file_path = nullptr;
                            hr = pItem->GetDisplayName(SIGDN_FILESYSPATH, &file_path);
                            if (SUCCEEDED(hr))
                            {
                                if (i > 0) allPaths += L"|";
                                allPaths += file_path;
                                CoTaskMemFree(file_path);
                            }
                            pItem->Release();
                        }
                    }

                    wcsncpy_s(outPath, 32767, allPaths.c_str(), _TRUNCATE);
                    result = true;
                    pItemArray->Release();
                }
            }
            else
            {
                IShellItem* p_item = nullptr;
                hr = openDialog->GetResult(&p_item);
                if (SUCCEEDED(hr))
                {
                    PWSTR file_path = nullptr;
                    hr = p_item->GetDisplayName(SIGDN_FILESYSPATH, &file_path);

                    if (SUCCEEDED(hr))
                    {
                        wcsncpy_s(outPath, 32767, file_path, _TRUNCATE);
                        CoTaskMemFree(file_path);
                        result = true;
                    }
                    p_item->Release();
                }
            }
        }
        openDialog->Release();
    }
}

__declspec(dllexport) void create_file_save_dialog(const wchar_t* title, const wchar_t* initialPath, wchar_t* outPath,
                                                   int fosFlags,
                                                   const wchar_t* filter, HRESULT& hr, BOOL& result)
{
    IFileSaveDialog* saveDialog = nullptr;
    hr = CoCreateInstance(CLSID_FileSaveDialog, nullptr, CLSCTX_ALL,
                          IID_IFileSaveDialog,
                          reinterpret_cast<void**>(&saveDialog));

    if (SUCCEEDED(hr))
    {
        DWORD dwOptions;
        hr = saveDialog->GetOptions(&dwOptions);
        if (SUCCEEDED(hr))
        {
            saveDialog->SetOptions(dwOptions | fosFlags);
        }

        if (title != nullptr && wcslen(title) > 0)
        {
            saveDialog->SetTitle(title);
        }

        if (initialPath != nullptr && wcslen(initialPath) > 0)
        {
            IShellItem* pInitialFolder = nullptr;
            hr = SHCreateItemFromParsingName(initialPath, nullptr, IID_PPV_ARGS(&pInitialFolder));
            if (SUCCEEDED(hr))
            {
                saveDialog->SetFolder(pInitialFolder);
                pInitialFolder->Release();
            }
        }

        std::wstring defaultExt = SetFileTypes(saveDialog, filter);

        if (!defaultExt.empty())
        {
            saveDialog->SetDefaultExtension(defaultExt.c_str());
        }

        hr = saveDialog->Show(nullptr);

        if (SUCCEEDED(hr))
        {
            IShellItem* p_item = nullptr;
            hr = saveDialog->GetResult(&p_item);
            if (SUCCEEDED(hr))
            {
                PWSTR file_path = nullptr;
                hr = p_item->GetDisplayName(SIGDN_FILESYSPATH, &file_path);

                if (SUCCEEDED(hr))
                {
                    wcsncpy_s(outPath, 32767, file_path, _TRUNCATE);
                    CoTaskMemFree(file_path);
                    result = true;
                }
                p_item->Release();
            }
        }
        saveDialog->Release();
    }
}

__declspec(dllexport) BOOL ShowDialog(
    const wchar_t* title,
    const wchar_t* initialPath,
    wchar_t* outPath,
    int fosFlags,
    const wchar_t* filter)
{
    HRESULT hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
    if (FAILED(hr)) return false;

    BOOL result = false;

    bool isSaveDialog = fosFlags & FOS_CREATEPROMPT || fosFlags & FOS_OVERWRITEPROMPT;

    if (isSaveDialog)
    {
        create_file_save_dialog(title, initialPath, outPath, fosFlags, filter, hr, result);
    }
    else
    {
        create_open_file_dialog(title, initialPath, outPath, fosFlags, filter, hr, result);
    }

    CoUninitialize();
    return result;
}
}
